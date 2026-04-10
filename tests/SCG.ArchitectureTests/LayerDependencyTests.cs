using NetArchTest.Rules;
using System.Reflection;

namespace SCG.ArchitectureTests;

public sealed class LayerDependencyTests
{
    // Domain assemblies
    private static readonly Assembly AgencyDomain = typeof(AgencyManagement.Domain.Entities.Agency).Assembly;
    private static readonly Assembly IdentityDomain = typeof(Identity.Domain.Entities.AdminUser).Assembly;
    private static readonly Assembly InquiryDomain = typeof(InquiryManagement.Domain.Entities.Batch).Assembly;
    private static readonly Assembly RulesDomain = typeof(Rules.Domain.Entities.Nationality).Assembly;

    // Application assemblies
    private static readonly Assembly AgencyApp = typeof(AgencyManagement.Application.Commands.RegisterAgency.RegisterAgencyCommand).Assembly;
    private static readonly Assembly IdentityApp = typeof(Identity.Application.Commands.Login.LoginCommand).Assembly;
    private static readonly Assembly InquiryApp = typeof(InquiryManagement.Application.Commands.CreateBatch.CreateBatchCommand).Assembly;
    private static readonly Assembly RulesApp = typeof(Rules.Application.Commands.AddNationality.AddNationalityCommand).Assembly;

    [Theory]
    [InlineData("AgencyManagement")]
    [InlineData("Identity")]
    [InlineData("InquiryManagement")]
    [InlineData("Rules")]
    public void DomainLayer_ShouldNotReference_InfrastructureLayer(string moduleName)
    {
        var domainAssembly = moduleName switch
        {
            "AgencyManagement" => AgencyDomain,
            "Identity" => IdentityDomain,
            "InquiryManagement" => InquiryDomain,
            "Rules" => RulesDomain,
            _ => throw new ArgumentException(moduleName)
        };

        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn($"SCG.{moduleName}.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain layer of {moduleName} should not depend on Infrastructure. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Theory]
    [InlineData("AgencyManagement")]
    [InlineData("Identity")]
    [InlineData("InquiryManagement")]
    [InlineData("Rules")]
    public void DomainLayer_ShouldNotReference_ApplicationLayer(string moduleName)
    {
        var domainAssembly = moduleName switch
        {
            "AgencyManagement" => AgencyDomain,
            "Identity" => IdentityDomain,
            "InquiryManagement" => InquiryDomain,
            "Rules" => RulesDomain,
            _ => throw new ArgumentException(moduleName)
        };

        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn($"SCG.{moduleName}.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain layer of {moduleName} should not depend on Application.");
    }

    [Theory]
    [InlineData("AgencyManagement")]
    [InlineData("Identity")]
    [InlineData("InquiryManagement")]
    [InlineData("Rules")]
    public void DomainLayer_ShouldNotReference_Presentation(string moduleName)
    {
        var domainAssembly = moduleName switch
        {
            "AgencyManagement" => AgencyDomain,
            "Identity" => IdentityDomain,
            "InquiryManagement" => InquiryDomain,
            "Rules" => RulesDomain,
            _ => throw new ArgumentException(moduleName)
        };

        var result = Types.InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOn("SCG.API")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"Domain layer of {moduleName} should not depend on API/Presentation.");
    }

    [Fact]
    public void DomainLayers_ShouldNotReference_EntityFramework()
    {
        var allDomainAssemblies = new[] { AgencyDomain, IdentityDomain, InquiryDomain, RulesDomain };

        foreach (var assembly in allDomainAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("Microsoft.EntityFrameworkCore")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Domain assembly {assembly.GetName().Name} should not reference EF Core.");
        }
    }

    [Fact]
    public void ApplicationLayers_ShouldNotReference_Presentation()
    {
        var allAppAssemblies = new[] { AgencyApp, IdentityApp, InquiryApp, RulesApp };

        foreach (var assembly in allAppAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOn("SCG.API")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"Application assembly {assembly.GetName().Name} should not reference API/Presentation.");
        }
    }
}
