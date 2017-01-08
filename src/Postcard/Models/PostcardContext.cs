using Microsoft.EntityFrameworkCore;
using System;

public class PostcardContext : DbContext
{
    public PostcardContext(DbContextOptions<PostcardContext> options)
        : base(options)
    { }

    public DbSet<PlaceNode> PlaceNodes { get; set; }
}
