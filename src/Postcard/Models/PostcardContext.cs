using Microsoft.EntityFrameworkCore;
using Postcard.Models;
using System;

public class PostcardContext : DbContext
{
    public PostcardContext(DbContextOptions<PostcardContext> options)
        : base(options)
    { }

    public DbSet<PlaceNode> PlaceNodes { get; set; }
    public DbSet<SearchToken> SearchTokens { get; set; }
}
