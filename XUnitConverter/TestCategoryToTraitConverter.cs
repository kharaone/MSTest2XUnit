// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XUnitConverter
{
    public sealed class TestCategoryToTraitConverter : ConverterBase
    {
        private readonly TestCategoryToTraitRewriter _toTraitRewriter = new TestCategoryToTraitRewriter();

        protected override Task<Solution> ProcessAsync(Document document, SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            var newNode = _toTraitRewriter.Visit(syntaxNode);
            if (newNode != syntaxNode)
            {
                document = document.WithSyntaxRoot(newNode);
                //document = Formatter.FormatAsync(document, cancellationToken: cancellationToken).Result;
            }

            return Task.FromResult(document.Project.Solution);
        }

        internal sealed class TestCategoryToTraitRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax syntaxNode)
            {
                var newAttributes = new SyntaxList<AttributeListSyntax>();

                foreach (var attributeList in syntaxNode.AttributeLists)
                {
                    var nodeToUpdates =
                    attributeList
                    .Attributes
                    .Where(attribute => attribute.Name.ToString().Equals("TestCategory")).ToList();

                    var localAttributeList = attributeList;
                    foreach (var nodeToUpdate in nodeToUpdates)
                    {
                        var categoryName = nodeToUpdate.ArgumentList.Arguments.ToString();
                        var traitAttribute =
                            nodeToUpdate.WithName(ParseName("Trait"))
                                .WithArgumentList(ParseAttributeArgumentList(string.Concat("(", @"""Category"",", categoryName, @")")));

                        localAttributeList =
                            (AttributeListSyntax)VisitAttributeList(
                                attributeList.RemoveNode(nodeToUpdate, SyntaxRemoveOptions.KeepLeadingTrivia));

                        newAttributes = newAttributes.Add(AttributeList().WithAttributes(SeparatedList(new List<AttributeSyntax> { traitAttribute })));
                        if(nodeToUpdates.Count() != attributeList.Attributes.Count)
                        {
                            newAttributes = newAttributes.Add(localAttributeList);
                        }
                    }
                    if (!nodeToUpdates.Any())
                    {
                        newAttributes = newAttributes.Add(localAttributeList);
                    }
                    
                }

                //Get the leading trivia (the newlines and comments)
                var leadTriv = syntaxNode.GetLeadingTrivia();
                syntaxNode = syntaxNode.WithAttributeLists(newAttributes);
                //Append the leading trivia to the method
                syntaxNode = syntaxNode.WithLeadingTrivia(leadTriv);
                return base.VisitMethodDeclaration(syntaxNode);
            }
        }
    }
}
