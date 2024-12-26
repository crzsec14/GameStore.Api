using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GenresEndpoints
{
    const string GetGenreEndpointName = "GetGenre";
    public static RouteGroupBuilder MapGenresEndpoints(this WebApplication app){
        var group = app.MapGroup("genres").WithParameterValidation();
        group.MapGet("/", async (GameStoreContext dbContext) => {
            return Results.Ok(await dbContext.Genres
                                             .Select(x => x.ToDto())
                                             .AsNoTracking()
                                             .ToListAsync());
        });

        // GET /games/{id}
        group.MapGet("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            var genre = await dbContext.Genres.FindAsync(id);

            return genre is null ? Results.NotFound() : Results.Ok(genre.ToDto());
        })
        .WithName(GetGenreEndpointName);

        // POST /games
        group.MapPost("/", async (GenreDto newGenre,
                            GameStoreContext dbContext) =>
        {
            var genre = newGenre.ToEntity();

            dbContext.Genres.Add(genre);
            await dbContext.SaveChangesAsync();

            return Results.CreatedAtRoute(GetGenreEndpointName, 
                                         new { id = genre.Id }, 
                                         genre.ToDto());
        });

        //PUT /games/{id}
        group.MapPut("/{id}", async (int id, GenreDto updateGenre, GameStoreContext dbContext) =>
        {
            var existingGenre = await dbContext.Genres.FindAsync(id);

            if (existingGenre is null)
            {
                return Results.NotFound();
            }

            dbContext.Entry(existingGenre)
                     .CurrentValues
                     .SetValues(updateGenre.ToEntity());
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        //DELETE /games/{id}
        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games
                     .Where(x => x.Id == id)
                     .ExecuteDeleteAsync();
                     
            return Results.NoContent();
        });

        return group;
    }

}
