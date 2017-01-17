using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Postcard.Migrations
{
    [DbContext(typeof(PostcardContext))]
    partial class PostcardContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PlaceNode", b =>
                {
                    b.Property<int>("PlaceNodeID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("PlaceName");

                    b.Property<string>("StateName");

                    b.Property<string>("link");

                    b.Property<int>("numberOfStatesThatHaveThisPlace");

                    b.HasKey("PlaceNodeID");

                    b.ToTable("PlaceNodes");
                });

            modelBuilder.Entity("Postcard.Models.SearchToken", b =>
                {
                    b.Property<string>("SearchTokenId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("SearchName");

                    b.HasKey("SearchTokenId");

                    b.ToTable("SearchTokens");
                });
        }
    }
}
