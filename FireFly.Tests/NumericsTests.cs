using System.Numerics;
using Xunit;

namespace FireFly.Tests;

public class NumericsTests {
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 30, 30)]
    [InlineData(30, 0, 30)]
    [InlineData(-30, 0, 30)]
    [InlineData(84, 30, 6)]
    [InlineData(30, 84, 6)]
    [InlineData(-84, 30, 6)]
    [InlineData(84, -30, 6)]
    [InlineData(-84, -30, 6)]
    [InlineData(17, 13, 1)]
    [InlineData(48, 48, 48)]
    [InlineData(1836311903, 1134903170, 1)]
    public void Gcd(int a, int b, int expected) {
        Assert.Equal(expected, Numerics.Gcd(a, b));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 30, 0)]
    [InlineData(30, 0, 0)]
    [InlineData(84, 30, 420)]
    [InlineData(30, 84, 420)]
    [InlineData(-84, 30, 420)]
    [InlineData(84, -30, 420)]
    [InlineData(-84, -30, 420)]
    [InlineData(17, 13, 221)]
    [InlineData(48, 48, 48)]
    [InlineData(1000000000, 2000000000, 2000000000)]
    public void Lcm(int a, int b, int expected) {
        Assert.Equal(expected, Numerics.Lcm(a, b));
    }

    [Fact]
    public void GcdAndLcmSupportDifferentNumericTypes() {
        Assert.Equal(6L, Numerics.Gcd(84L, 30L));
        Assert.Equal(420L, Numerics.Lcm(84L, 30L));
        Assert.Equal(6UL, Numerics.Gcd(84UL, 30UL));
        Assert.Equal(420UL, Numerics.Lcm(84UL, 30UL));
        Assert.Equal((Int128)6, Numerics.Gcd((Int128)84, (Int128)30));
        Assert.Equal((Int128)420, Numerics.Lcm((Int128)84, (Int128)30));

        BigInteger a = BigInteger.One << 150;
        Assert.Equal(3 * a, Numerics.Gcd(6 * a, 15 * a));
        Assert.Equal(30 * a, Numerics.Lcm(6 * a, 15 * a));
    }
}
