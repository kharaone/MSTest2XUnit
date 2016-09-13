// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    public sealed class ExpectedExceptionConverter : ConverterBase
    {
        private readonly ExpectedExceptionRewriter _rewriter = new ExpectedExceptionRewriter();

        protected override Task<Solution> ProcessAsync(Document document, SyntaxNode syntaxNode, CancellationToken cancellationToken)
        {
            var newNode = _rewriter.Visit(syntaxNode);
            if (newNode != syntaxNode)
            {
                document = document.WithSyntaxRoot(newNode);
            }
            document = Formatter.FormatAsync(document, cancellationToken: cancellationToken).Result;
            return Task.FromResult(document.Project.Solution);
        }

        internal sealed class ExpectedExceptionRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax syntaxNode)
            {
                var newAttributes = new SyntaxList<AttributeListSyntax>();
                var isMethodToRefactor = false;
                IdentifierNameSyntax expectedException=IdentifierName("");
                        

                foreach (var attributeList in syntaxNode.AttributeLists)
                {
                    var nodesToRemove =
                        attributeList
                            .Attributes
                            .Where(attribute => attribute.Name.ToString().Equals("ExpectedException"))
                            .ToArray();

                    //If the lists are the same length, we are removing all attributes and can just avoid populating newAttributes.
                    if (nodesToRemove.Length >0 )
                    {
                        isMethodToRefactor = true;
                        expectedException =
                        nodesToRemove.First().ArgumentList.DescendantNodes().OfType<IdentifierNameSyntax>().First();
                        if (nodesToRemove.Length != attributeList.Attributes.Count)
                        {
                            var newAttribute =
                            (AttributeListSyntax)VisitAttributeList(
                                attributeList.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia));

                            newAttributes = newAttributes.Add(newAttribute);
                        }
                            
                    }
                    else
                    {
                        newAttributes = newAttributes.Add(attributeList);
                    }
                    
                }

                //Get the leading trivia (the newlines and comments)
                var leadTriv = syntaxNode.GetLeadingTrivia();
                syntaxNode = syntaxNode.WithAttributeLists(newAttributes);
                //Append the leading trivia to the method
                syntaxNode = syntaxNode.WithLeadingTrivia(leadTriv);

                if (isMethodToRefactor)
                {
                    var oldBody = syntaxNode.Body;
                    var newBody = Block(ParseStatement("Assert.Throws<" + expectedException.Identifier + ">(()=>"+oldBody+");"));

                    leadTriv = syntaxNode.GetLeadingTrivia();
                    syntaxNode = syntaxNode.WithBody(newBody);
                    //Append the leading trivia to the method
                    syntaxNode = syntaxNode.WithLeadingTrivia(leadTriv)
                        .WithModifiers(
                            TokenList(
                                Token(
                                    TriviaList(
                                        Comment("//HACK: Naive implementation of ExpectedException in XUnit")),
                                    SyntaxKind.PublicKeyword,
                                    TriviaList()).NormalizeWhitespace()));
                }


                //Get the leading trivia (the newlines and comments)
                
                return base.VisitMethodDeclaration(syntaxNode);
            }
        }
    }
}
