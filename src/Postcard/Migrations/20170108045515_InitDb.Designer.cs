using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Postcard.Migrations
{
    [DbContext(typeof(PostcardContext))]
    [Migration("20170108045515_InitDb")]
    partial class InitDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
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
        }
    }
}
