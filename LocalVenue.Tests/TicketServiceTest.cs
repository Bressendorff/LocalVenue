﻿using AutoMapper;
using LocalVenue.Core;
using LocalVenue.Core.Entities;
using LocalVenue.Core.Enums;
using LocalVenue.Core.Services;
using LocalVenue.Helpers;
using LocalVenue.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Customer = LocalVenue.Core.Entities.Customer;

namespace LocalVenue.Tests;
public class TicketServiceTest
{
    private readonly ServiceProvider serviceProvider;
    
    
    public TicketServiceTest()
    {
        var services = new ServiceCollection();
        services.AddDbContextFactory<VenueContext>(options =>
        {
            options.UseInMemoryDatabase("InMemoryDb")
                .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        });
        
        serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task TestTicketServiceBuySeat()
    {
        // Arrange
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();
        Customer customer = new Customer
        {
            Id = "0c9cd65f-2054-4086-a569-2e50997a8be9",
            UserName = "Nicolas",
            NormalizedUserName = "NICOLAS",
            Email = "nicolas.mousten@gmail.com",
            NormalizedEmail = "NICOLAS.MOUSTEN@GMAIL.COM",
            EmailConfirmed = false,
            PasswordHash = "AQAAAAIAAYagAAAAEJWgg4FDKFWNh/AYIzVE/3nxRluYnwDmUfDnpc75ZUylWzJYkphFBrhqFkRAgm16YA==",
            SecurityStamp = "S4KC54SOPWVKCI7KA6MGFCDMBS5SVWXG",
            ConcurrencyStamp = "9ceff3cb-b5c8-4562-bcef-4a4f0d3c3761",
            PhoneNumber = null,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnd = null,
            LockoutEnabled = false,
            AccessFailedCount = 0,
            FirstName = "Nicolas",
            LastName = "Mousten",
            IsGoldenMember = false,
            Tickets = new List<Ticket>()
        };
        Show show = new Show
        {
            ShowId = 1,
            Title = "TestShow",
            Description = "A show to test on",
            StartTime = DateTime.Now.AddDays(1).AddHours(12),
            EndTime = DateTime.Now.AddDays(1).AddHours(14),
            Genre = Genre.Musical,
            OpeningNight = false,
            Tickets = new List<Ticket>()
        };

        Seat seat = new Seat
        {
            SeatId = 4,
            Section = "Front",
            Row = 1,
            Number = 1
        };

        Ticket ticket = new Ticket
        {
            TicketId = 1,
            ShowId = show.ShowId,
            Show = show,
            SeatId = seat.SeatId,
            Seat = seat,
            Price = 50,
            Status = Status.Available,
            CustomerId = null,
            Customer = null
        };
        
        show.Tickets.Add(ticket);
        await using (var context = await dbContextFactory.CreateDbContextAsync())
        {
            context.Users.Add(customer);
            context.Shows.Add(show);
            await context.SaveChangesAsync();
        }
        // Act
        var contextFactoryRetrieve = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();
        
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();  // Assuming ShowProfile contains your mappings
        });
        var mapper = config.CreateMapper();
        
        var ticketService = new TicketService(contextFactoryRetrieve, mapper);
        var seatService = new SeatService(contextFactoryRetrieve);
        var showService = new ShowService(contextFactoryRetrieve, mapper);

        var fetchShow = await showService.GetShowWithTicketsAsync(1);
        
        await ticketService.JoinShow(fetchShow.Id, fetchShow.Tickets, "0c9cd65f-2054-4086-a569-2e50997a8be9");
        var result = await ticketService.GetTicket(1);
        // Assert
        Assert.NotNull(result);
        Assert.Equal("0c9cd65f-2054-4086-a569-2e50997a8be9", result.CustomerId);
        Assert.Equal(Status.Sold, result.Status);
    }
    
    [Fact]
    public async Task TestTicketServiceReturnSeat()
    {
        // Arrange
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();

        Customer customer = new Customer
        {
            Id = "0c9cd65f-2054-4086-a569-2e50997a8be3",
            UserName = "Nicolas",
            NormalizedUserName = "NICOLAS",
            Email = "nicolas.mousten@gmail.com",
            NormalizedEmail = "NICOLAS.MOUSTEN@GMAIL.COM",
            EmailConfirmed = false,
            PasswordHash = "AQAAAAIAAYagAAAAEJWgg4FDKFWNh/AYIzVE/3nxRluYnwDmUfDnpc75ZUylWzJYkphFBrhqFkRAgm16YA==",
            SecurityStamp = "S4KC54SOPWVKCI7KA6MGFCDMBS5SVWXG",
            ConcurrencyStamp = "9ceff3cb-b5c8-4562-bcef-4a4f0d3c3761",
            PhoneNumber = null,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnd = null,
            LockoutEnabled = false,
            AccessFailedCount = 0,
            FirstName = "Nicolas",
            LastName = "Mousten",
            IsGoldenMember = false,
            Tickets = new List<Ticket>()
        };
        Show show = new Show
        {
            ShowId = 3,
            Title = "TestShow",
            Description = "A show to test on",
            StartTime = DateTime.Now.AddHours(12),
            EndTime = DateTime.Now.AddHours(14),
            Genre = Genre.Musical,
            OpeningNight = false,
            Tickets = new List<Ticket>()
        };
        Seat seat = new Seat
        {
            SeatId = 3,
            Section = "Front",
            Row = 1,
            Number = 1
        };
        Ticket ticket = new Ticket
        {
            TicketId = 3,
            ShowId = show.ShowId,
            Show = show,
            SeatId = 3,
            Seat = seat,
            Price = 50,
            Status = Status.Sold,
            CustomerId = "0c9cd65f-2054-4086-a569-2e50997a8be9",
            Customer = customer
        };

        show.Tickets?.Add(ticket);
        await using (var context = await dbContextFactory.CreateDbContextAsync())
        {
            context.Users.Add(customer);
            context.Shows.Add(show);
            await context.SaveChangesAsync();
        }

        // Act
        var contextFactoryRetrieve = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        var mapper = config.CreateMapper();
        
        var ticketService = new TicketService(contextFactoryRetrieve, mapper);
        var seatService = new SeatService(contextFactoryRetrieve);
        var showService = new ShowService(contextFactoryRetrieve, mapper);


        var fetchShow = await showService.GetShowWithTicketsAsync(1);
            
        await ticketService.LeaveShow(fetchShow.Id, fetchShow.Tickets, "0c9cd65f-2054-4086-a569-2e50997a8be3");
        var result = await ticketService.GetTicket(1);
        // Assert
        Assert.NotNull(result);
        Assert.Null(result.CustomerId);
        //Assert.Equal(null, result.CustomerId);
        Assert.Equal(Status.Available, result.Status);
    }
    
    [Fact(DisplayName = "Test if the ticket status remains 'Sold' after the customer tries to leave the show less than 24 hours before it begins")]
    public async Task TestTicketServiceReturnSeatOutOfDate()
    {
        // Arrange
        var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();

        Customer customer = new Customer
        {
            Id = "0c9cd65f-2054-4086-a569-2e50997a8be6",
            UserName = "Nicolas",
            NormalizedUserName = "NICOLAS",
            Email = "nicolas.mousten@gmail.com",
            NormalizedEmail = "NICOLAS.MOUSTEN@GMAIL.COM",
            EmailConfirmed = false,
            PasswordHash = "AQAAAAIAAYagAAAAEJWgg4FDKFWNh/AYIzVE/3nxRluYnwDmUfDnpc75ZUylWzJYkphFBrhqFkRAgm16YA==",
            SecurityStamp = "S4KC54SOPWVKCI7KA6MGFCDMBS5SVWXG",
            ConcurrencyStamp = "9ceff3cb-b5c8-4562-bcef-4a4f0d3c3761",
            PhoneNumber = null,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnd = null,
            LockoutEnabled = false,
            AccessFailedCount = 0,
            FirstName = "Nicolas",
            LastName = "Mousten",
            IsGoldenMember = false,
            Tickets = new List<Ticket>()
        };
        Show show = new Show
        {
            ShowId = 2,
            Title = "TestShow",
            Description = "A show to test on",
            StartTime = DateTime.Now.AddHours(12),
            EndTime = DateTime.Now.AddHours(14),
            Genre = Genre.Musical,
            OpeningNight = false,
            Tickets = new List<Ticket>()
        };
        Seat seat = new Seat
        {
            SeatId = 2,
            Section = "Front",
            Row = 1,
            Number = 1
        };
        Ticket ticket = new Ticket
        {
            TicketId = 2,
            ShowId = show.ShowId,
            Show = show,
            SeatId = 2,
            Seat = seat,
            Price = 50,
            Status = Status.Sold,
            CustomerId = "0c9cd65f-2054-4086-a569-2e50997a8be6",
            Customer = customer
        };

        show.Tickets?.Add(ticket);
        await using (var context = await dbContextFactory.CreateDbContextAsync())
        {
            context.Users.Add(customer);
            context.Shows.Add(show);
            await context.SaveChangesAsync();
        }

        // Act
        var contextFactoryRetrieve = serviceProvider.GetRequiredService<IDbContextFactory<VenueContext>>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        var mapper = config.CreateMapper();
        
        var ticketService = new TicketService(contextFactoryRetrieve, mapper);
        var seatService = new SeatService(contextFactoryRetrieve);
        var showService = new ShowService(contextFactoryRetrieve, mapper);


        var fetchShow = await showService.GetShowWithTicketsAsync(2);
            
        await ticketService.LeaveShow(fetchShow.Id, fetchShow.Tickets, "0c9cd65f-2054-4086-a569-2e50997a8be6");
        var result = await ticketService.GetTicket(2);
        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.CustomerId);
        Assert.Equal("0c9cd65f-2054-4086-a569-2e50997a8be6", result.CustomerId); 
        Assert.Equal(Status.Sold, result.Status);
    }
}