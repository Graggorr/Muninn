using System.Security.Cryptography;
using System.Text;
using AutoFixture;
using Muninn.Kernel.Models;

namespace Muninn.Tests.Shared;

public static class EntryCreator
{
    public static Entry CreateRandomEntry()
    {
        var encoding = Encoding.UTF8;
        var valueBuilder = new StringBuilder()
            .Append(Guid.CreateVersion7().ToString())
            .Append(Guid.CreateVersion7().ToString())
            .Append(Guid.CreateVersion7().ToString())
            .Append(Guid.CreateVersion7().ToString())
            .Append(Guid.CreateVersion7().ToString());
        var value = encoding.GetBytes(valueBuilder.ToString());

        return new(Guid.CreateVersion7().ToString(), value, encoding)
        {
            LifeTime = TimeSpan.FromDays(1),
        };
    }

    public static Entry CreateFixtureEntry()
    {
        var fixture = new Fixture();
        fixture.Customize<Entry>(customization => customization
            .With(entry => entry.Key, Guid.CreateVersion7().ToString())
            .With(entry => entry.LifeTime, TimeSpan.FromDays(1))
            .With(entry => entry.CreationTime, DateTime.Now)
            .With(entry => entry.LastModificationTime, DateTime.Now)
        );

        return fixture.Create<Entry>();
    }
}