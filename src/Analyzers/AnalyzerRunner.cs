﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.CodeAnalysis.Tools.Analyzers
{
    internal partial class AnalyzerRunner : IAnalyzerRunner
    {
        public Task RunCodeAnalysisAsync(CodeAnalysisResult result,
                                         DiagnosticAnalyzer analyzer,
                                         Project project,
                                         AnalyzerOptions analyzerOptions,
                                         ImmutableArray<string> formattableDocumentPaths,
                                         ILogger logger,
                                         CancellationToken cancellationToken)
            => RunCodeAnalysisAsync(result, ImmutableArray.Create(analyzer), project, analyzerOptions, formattableDocumentPaths, logger, cancellationToken);

        public async Task RunCodeAnalysisAsync(CodeAnalysisResult result,
                                         ImmutableArray<DiagnosticAnalyzer> analyzers,
                                         Project project,
                                         AnalyzerOptions analyzerOptions,
                                         ImmutableArray<string> formattableDocumentPaths,
                                         ILogger logger,
                                         CancellationToken cancellationToken)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken);
            var analyzerCompilation = compilation.WithAnalyzers(
                analyzers,
                options: analyzerOptions,
                cancellationToken);
            var diagnostics = await analyzerCompilation.GetAnalyzerDiagnosticsAsync(cancellationToken);
            // filter diagnostics
            foreach (var diagnostic in diagnostics)
            {
                if (!diagnostic.IsSuppressed &&
                    diagnostic.Severity >= DiagnosticSeverity.Warning &&
                    diagnostic.Location.IsInSource &&
                    formattableDocumentPaths.Contains(diagnostic.Location.SourceTree.FilePath, StringComparer.OrdinalIgnoreCase))
                {
                    result.AddDiagnostic(project, diagnostic);
                }
            }
        }
    }
}
