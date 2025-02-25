﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.QuickInfo;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace Microsoft.CodeAnalysis.LanguageServer.Handler
{
    internal interface ILspHoverResultCreationService : IWorkspaceService
    {
        Task<Hover> CreateHoverAsync(
            SourceText text, string language, QuickInfoItem info, Document? document, ClientCapabilities? clientCapabilities, CancellationToken cancellationToken);
    }

    [ExportWorkspaceService(typeof(ILspHoverResultCreationService)), Shared]
    internal sealed class DefaultLspHoverResultCreationService : ILspHoverResultCreationService
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public DefaultLspHoverResultCreationService()
        {
        }

        public Task<Hover> CreateHoverAsync(SourceText text, string language, QuickInfoItem info, Document? document, ClientCapabilities? clientCapabilities, CancellationToken cancellationToken)
            => Task.FromResult(CreateDefaultHover(text, language, info, clientCapabilities));

        public static Hover CreateDefaultHover(SourceText text, string language, QuickInfoItem info, ClientCapabilities? clientCapabilities)
        {
            var clientSupportsMarkdown = clientCapabilities?.TextDocument?.Hover?.ContentFormat.Contains(MarkupKind.Markdown) == true;

            // Insert line breaks in between sections to ensure we get double spacing between sections.
            var tags = info.Sections
                .SelectMany(section => section.TaggedParts.Add(new TaggedText(TextTags.LineBreak, Environment.NewLine)))
                .ToImmutableArray();

            return new Hover
            {
                Range = ProtocolConversions.TextSpanToRange(info.Span, text),
                Contents = ProtocolConversions.GetDocumentationMarkupContent(tags, language, clientSupportsMarkdown),
            };
        }
    }
}
