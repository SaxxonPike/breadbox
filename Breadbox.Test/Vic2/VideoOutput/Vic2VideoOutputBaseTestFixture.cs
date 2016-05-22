using FluentAssertions;
using NUnit.Framework;

namespace Breadbox.Test.Vic2.VideoOutput
{
    public abstract class Vic2VideoOutputBaseTestFixture : Vic2BaseTestFixture
    {
        [SetUp]
        public void InitializeFrameBuffer()
        {
            EnableFrameBuffer();
        }

        [Test]
        public void ClockFrame_ShouldRenderFullFrame()
        {
            // Act
            Vic.ClockFrame();

            // Assert
            PixelsOutputToFrameBuffer.Should().Be(Config.VisibleRasterLines * Config.VisiblePixelsPerRasterLine);
        }
    }
}
