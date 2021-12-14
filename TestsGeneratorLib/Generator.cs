﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsGeneratorLib
{
    public class Generator
    {
        public List<TestUnit> CreateTests(string source)
        {
            List<TestUnit> list = new List<TestUnit>();
            SyntaxNode root = CSharpSyntaxTree.ParseText(source).GetRoot();
            foreach (ClassDeclarationSyntax syntax in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                ClassDeclarationSyntax testClass = CreateTestClass(syntax.Identifier.ValueText);
                IEnumerable<MethodDeclarationSyntax> methods = syntax.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    .Where(method => method.Modifiers.Any(SyntaxKind.PublicKeyword));
                foreach (MethodDeclarationSyntax method in methods)
                {
                    testClass = testClass.AddMembers(CreateTestMethod(method.Identifier.ValueText));
                }

                CompilationUnitSyntax unit = CompilationUnit().WithUsings(GetImports())
                    .AddMembers(NamespaceDeclaration(ParseName("Tests")).AddMembers(testClass));
                list.Add(new TestUnit($"{syntax.Identifier.ValueText}Tests.cs",
                    unit.NormalizeWhitespace().ToFullString()));
            }




            return list;
        }

        public SyntaxList<UsingDirectiveSyntax> GetImports()
        {
            List<UsingDirectiveSyntax> defaultUsages = new List<UsingDirectiveSyntax>
            {
                UsingDirective(QualifiedName(
                    QualifiedName(QualifiedName(IdentifierName("Microsoft"), IdentifierName("VisualStudio")),
                        IdentifierName("TestTools")), IdentifierName("UnitTesting")))
            };
            return List(defaultUsages);
        }

        private ClassDeclarationSyntax CreateTestClass(string className)
        {
            AttributeSyntax attribute = Attribute(ParseName("TestClass"));
            return ClassDeclaration(className + "Test")
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAttributeLists(AttributeList().AddAttributes(attribute));
        }


        private MethodDeclarationSyntax CreateTestMethod(string methodName)
        {
            AttributeSyntax attribute = Attribute(ParseName("TestMethod"));
            return MethodDeclaration(ParseTypeName("void"), methodName + "Test")
                .AddModifiers(Token(SyntaxKind.PublicKeyword)).AddBodyStatements(EmptyTestSyntax())
                .AddAttributeLists(AttributeList().AddAttributes(attribute));
        }

        private StatementSyntax[] EmptyTestSyntax()
        {
            return new[] { ParseStatement("Assert.Fail(\"autogenerated\");") };
        }

    }
}
