﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.VisualStudio.LanguageServices.FindUsages
{
    /// <summary>
    /// Base type of all <see cref="Entry"/>s that represent some source location in 
    /// a <see cref="Document"/>.  Navigation to that location is provided by this type.
    /// Subclasses can be used to provide customized line text to display in the entry.
    /// </summary>
    internal abstract class AbstractDocumentSpanEntry : Entry
    {
        private readonly AbstractTableDataSourceFindUsagesContext _context;

        private readonly DocumentSpan _documentSpan;
        private readonly object _boxedProjectGuid;
        protected readonly SourceText _sourceText;

        protected AbstractDocumentSpanEntry(
            AbstractTableDataSourceFindUsagesContext context,
            RoslynDefinitionBucket definitionBucket,
            DocumentSpan documentSpan,
            Guid projectGuid,
            SourceText sourceText)
            : base(definitionBucket)
        {
            _context = context;

            _documentSpan = documentSpan;
            _boxedProjectGuid = projectGuid;
            _sourceText = sourceText;
        }

        protected StreamingFindUsagesPresenter Presenter => _context.Presenter;

        protected Document Document => _documentSpan.Document;
        protected TextSpan SourceSpan => _documentSpan.SourceSpan;

        protected override object GetValueWorker(string keyName)
        {
            switch (keyName)
            {
                case StandardTableKeyNames.DocumentName:
                    return Document.FilePath;
                case StandardTableKeyNames.Line:
                    return _sourceText.Lines.GetLinePosition(SourceSpan.Start).Line;
                case StandardTableKeyNames.Column:
                    return _sourceText.Lines.GetLinePosition(SourceSpan.Start).Character;
                case StandardTableKeyNames.ProjectName:
                    return Document.Project.Name;
                case StandardTableKeyNames.ProjectGuid:
                    return _boxedProjectGuid;
                case StandardTableKeyNames.Text:
                    return _sourceText.Lines.GetLineFromPosition(SourceSpan.Start).ToString().Trim();
            }

            return null;
        }

        public sealed override bool TryCreateColumnContent(string columnName, out FrameworkElement content)
        {
            if (columnName == StandardTableColumnDefinitions2.LineText)
            {
                var inlines = CreateLineTextInlines();
                var textBlock = inlines.ToTextBlock(Presenter.TypeMap, wrap: false);

                content = textBlock;
                return true;
            }

            content = null;
            return false;
        }

        protected abstract IList<Inline> CreateLineTextInlines();
    }
}