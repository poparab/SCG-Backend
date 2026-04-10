using NetArchTest.Rules;
using SCG.Application.Abstractions.Messaging;
using System.Reflection;

namespace SCG.ArchitectureTests;

public sealed class NamingConventionTests
{
    private static readonly Assembly[] ApplicationAssemblies =
    [
        typeof(AgencyManagement.Application.Commands.RegisterAgency.RegisterAgencyCommand).Assembly,
        typeof(Identity.Application.Commands.Login.LoginCommand).Assembly,
        typeof(InquiryManagement.Application.Commands.CreateBatch.CreateBatchCommand).Assembly,
        typeof(Rules.Application.Commands.AddNationality.AddNationalityCommand).Assembly,
    ];

    [Fact]
    public void Commands_ShouldEndWithCommand()
    {
        foreach (var assembly in ApplicationAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(ICommand<>))
                .Or()
                .ImplementInterface(typeof(ICommand))
                .Should()
                .HaveNameEndingWith("Command")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"All commands should end with 'Command'. Failing: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Fact]
    public void Queries_ShouldEndWithQuery()
    {
        foreach (var assembly in ApplicationAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(IQuery<>))
                .Should()
                .HaveNameEndingWith("Query")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"All queries should end with 'Query'. Failing: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    [Fact]
    public void CommandHandlers_ShouldEndWithCommandHandler()
    {
        foreach (var assembly in ApplicationAssemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .ImplementInterface(typeof(ICommandHandler<>))
                .Or()
                .ImplementInterface(typeof(ICommandHandler<,>))
                .Should()
                .HaveNameEndingWith("CommandHandler")
                .GetResult();

            result.IsSuccessful.Should().BeTrue(
                $"All command handlers should end with 'CommandHandler'. Failing: {string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }
}
