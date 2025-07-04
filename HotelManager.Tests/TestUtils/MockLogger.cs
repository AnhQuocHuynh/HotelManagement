using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace HotelManager.Tests.TestUtils
{
    public static class MockLogger
    {
        public static Mock<ILogger<T>> Create<T>()
        {
            var logger = new Mock<ILogger<T>>();
            logger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();
            logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            return logger;
        }
    }
} 