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
using Microsoft.CodeAnalysis.MSBuild;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace XUnitConverter
{
    public sealed class IgnoreToSkipConverter : ConverterBase
    {
        private readonly IgnoreToSkipRewriter _rewriter = new IgnoreToSkipRewriter();

        protected override Task<Solution> ProcessAsync(Document document, SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            var newNode = _rewriter.Visit(syntaxNode);
            if (newNode != syntaxNode)
            {
                document = document.WithSyntaxRoot(newNode);
                //document = Formatter.FormatAsync(document, cancellationToken: cancellationToken).Result;
            }

            return Task.FromResult(document.Project.Solution);
        }

        internal sealed class IgnoreToSkipRewriter : CSharpSyntaxRewriter
        {

            public bool IsClassIgnored { get; set; }

           

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax syntaxNode)
            {
                var newAttributes = new SyntaxList<AttributeListSyntax>();
                var isMethodIgnored = false;

                foreach (var attributeList in syntaxNode.AttributeLists)
                {
                    var nodesToRemove =
                        attributeList
                            .Attributes
                            .Where(attribute => attribute.Name.ToString().Equals("Ignore"))
                            .ToArray();

                    //If the lists are the same length, we are removing all attributes and can just avoid populating newAttributes.
                    if(nodesToRemove.Length >0 ) isMethodIgnored = true;
                    if (nodesToRemove.Length != attributeList.Attributes.Count)
                    {
                        var newAttribute =
                            (AttributeListSyntax) VisitAttributeList(
                                attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia));

                        newAttributes = newAttributes.Add(newAttribute);
                    }
                    
                }

                //Get the leading trivia (the newlines and comments)
                var leadTriv = syntaxNode.GetLeadingTrivia();
                syntaxNode = syntaxNode.WithAttributeLists(newAttributes);
                //Append the leading trivia to the method
                syntaxNode = syntaxNode.WithLeadingTrivia(leadTriv);

                newAttributes = new SyntaxList<AttributeListSyntax>();

                foreach (var attributeList in syntaxNode.AttributeLists)
                {
                    var nodeToUpdate =
                    attributeList
                    .Attributes
                    .FirstOrDefault(attribute => attribute.Name.ToString().Equals("Fact"));

                    if (nodeToUpdate != null && (isMethodIgnored|| IsClassIgnored))
                    {
                        var skippedAttribute = nodeToUpdate.WithArgumentList(
                                AttributeArgumentList(
                                    new SeparatedSyntaxList<AttributeArgumentSyntax>().Add(
                                        AttributeArgument(NameEquals("Skip"), null,
                                            LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                Token(TriviaList(), SyntaxKind.StringLiteralToken,
                                                    @"""Ignored in MSTest""", "Ignored in MSTest", TriviaList()))))));
                        var newAttribute =
                            (AttributeListSyntax)VisitAttributeList(
                                attributeList.ReplaceNode(nodeToUpdate, skippedAttribute));

                        newAttributes = newAttributes.Add(newAttribute);
                    }
                    else
                    {
                        newAttributes = newAttributes.Add(attributeList);
                    }
                }

                //Get the leading trivia (the newlines and comments)
                leadTriv = syntaxNode.GetLeadingTrivia();
                syntaxNode = syntaxNode.WithAttributeLists(newAttributes);
                //Append the leading trivia to the method
                syntaxNode = syntaxNode.WithLeadingTrivia(leadTriv);
                return base.VisitMethodDeclaration(syntaxNode); ;
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax syntaxNode)
            {
                IsClassIgnored =
                    syntaxNode.AttributeLists.Any(
                        attributeList => attributeList.Attributes.Any(a => a.Name.ToString().Equals("Ignore")));

                var newAttributes = new SyntaxList<AttributeListSyntax>();
             

                foreach (var attributeList in syntaxNode.AttributeLists)
                {
                    var nodesToRemove =
                        attributeList
                            .Attributes
                            .Where(attribute => attribute.Name.ToString().Equals("Ignore"))
                            .ToArray();

                    if (nodesToRemove.Length != attributeList.Attributes.Count)
                    {
                        var newAttribute =
                            (AttributeListSyntax)VisitAttributeList(
                                attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia));

                        newAttributes = newAttributes.Add(newAttribute);
                    }

                }

                //Get the leading trivia (the newlines and comments)
                var leadTriv = syntaxNode.GetLeadingTrivia();
                syntaxNode = syntaxNode.WithAttributeLists(newAttributes);
                //Append the leading trivia to the method
                syntaxNode = syntaxNode.WithLeadingTrivia(leadTriv);

                return base.VisitClassDeclaration(syntaxNode);
            }
        }
    }
}
