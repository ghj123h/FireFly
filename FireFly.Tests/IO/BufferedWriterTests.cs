using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text;
using FireFly.IO;
using Xunit;

namespace FireFly.Tests.IO;

public class BufferedWriterTests {
    [Fact]
    public void AppendIntegers() {
        CheckWriter(w => {
            w.Append(0);
            w.Append(int.MinValue);
            w.Append(ulong.MaxValue);
            w.Append(BigInteger.Parse("-123456789012345678901234567890"));
        }, "0-214748364818446744073709551615-123456789012345678901234567890");
    }

    [Fact]
    public void AppendString() {
        CheckWriter(w => w.Append("萤火🔥"), "萤火🔥");
    }

    [Fact]
    public void AppendLine() {
        CheckWriter(w => w.AppendLine(), "\n");
        CheckWriter(w => {
            w.AppendLine(0);
            w.AppendLine(int.MinValue);
            w.AppendLine(ulong.MaxValue);
            w.AppendLine(BigInteger.Parse("-123456789012345678901234567890"));
        }, "0\n-2147483648\n18446744073709551615\n-123456789012345678901234567890\n");

        string s = new string('x', (1 << 16) + 1) + "萤火";
        CheckWriter(w => w.AppendLine(s), s + "\n");
    }

    [Fact]
    public void AppendYesNo() {
        CheckWriter(w => w.AppendYes(), "Yes\n");
        CheckWriter(w => w.AppendNo(), "No\n");
        CheckWriter(w => {
            w.AppendYes(true);
            w.AppendYes(false);
        }, "Yes\nNo\n");
    }

    [Fact]
    public void AppendJoin() {
        IEnumerable<int> values = new[] { 1, -2, 3 };
        CheckWriter(w => w.AppendJoin(values), "1 -2 3\n");
        CheckWriter(w => w.AppendJoin('界', values), "1界-2界3\n");
        CheckWriter(w => w.AppendJoin(" | ", values), "1 | -2 | 3\n");
        CheckWriter(w => {
            ReadOnlySpan<short> span = new short[] { -4, 0, 5 };
            w.AppendJoin(span);
        }, "-4 0 5\n");
        CheckWriter(w => {
            ReadOnlySpan<ulong> span = new ulong[] { 6, ulong.MaxValue };
            w.AppendJoin(',', span);
        }, "6,18446744073709551615\n");
        CheckWriter(w => w.AppendJoin(Array.Empty<int>()), "\n");
    }

    [Fact]
    public void AppendFormatIsObsolete() {
#pragma warning disable CS0612, CS0618
        CheckWriter(w => w.AppendFormat("{0}", 42), "");
#pragma warning restore CS0612, CS0618
        MethodInfo format = typeof(BufferedWriter).GetMethod(nameof(BufferedWriter.AppendFormat))!;
        Assert.NotNull(format.GetCustomAttribute<ObsoleteAttribute>());
    }

    [Fact]
    public void IntegerApisHaveRequiredShape() {
        MethodInfo[] methods = typeof(BufferedWriter).GetMethods();
        Assert.DoesNotContain(methods, method => {
            ParameterInfo[] p = method.GetParameters();
            return (method.Name == nameof(BufferedWriter.Append) ||
                    method.Name == nameof(BufferedWriter.AppendLine)) &&
                !method.IsGenericMethod &&
                p.Length == 1 && p[0].ParameterType == typeof(object);
        });

        foreach (MethodInfo method in methods) {
            if (!method.IsGenericMethod ||
                method.Name != nameof(BufferedWriter.Append) &&
                method.Name != nameof(BufferedWriter.AppendLine) &&
                method.Name != nameof(BufferedWriter.AppendJoin)) continue;
            Type arg = method.GetGenericArguments()[0];
            Assert.Contains(arg.GetGenericParameterConstraints(), constraint =>
                constraint.IsGenericType &&
                constraint.GetGenericTypeDefinition() == typeof(IBinaryInteger<>));
        }
    }

    [Fact]
    public void OutputBuffersAndCanRepeat() {
        using MemoryStream stream = new();
        BufferedWriter writer = new(stream);
        writer.Append("first");
        Assert.Equal(0L, stream.Length);
        writer.Output();
        writer.Append("second");
        writer.Output();
        Assert.Equal("firstsecond", Encoding.UTF8.GetString(stream.ToArray()));
    }

    private static void CheckWriter(Action<BufferedWriter> action, string expected) {
        using MemoryStream stream = new();
        BufferedWriter writer = new(stream);
        action(writer);
        Assert.Equal(0L, stream.Length);
        writer.Output();
        Assert.Equal(expected, Encoding.UTF8.GetString(stream.ToArray()));
    }
}
