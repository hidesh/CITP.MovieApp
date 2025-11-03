using CITP.MovieApp.Api.Controllers;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CITP.MovieApp.Tests_
{
    public class MoviesControllerTests
    {
        private readonly Mock<IMovieRepository> _movieRepoMock;
        private readonly MoviesController _controller;

        public MoviesControllerTests()
        {
            _movieRepoMock = new Mock<IMovieRepository>();
            _controller = new MoviesController(_movieRepoMock.Object);
        }

    }
}