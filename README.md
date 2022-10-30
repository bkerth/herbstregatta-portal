# Management Portal for TRV Herbstregatta

This is a management portal designed for the Tuebinger Herbstregatta (See: [Homepage - Tübinger Herbstregatta](https://www.tübinger-herbstregatta.de))

__Following key features are available__
- Admins can create new races
- Admins can schedule races
- Admins can take the time of a race
- Teams can create an account and register for a race

__Planned features__
- More export functionalities
- Improved logging / authentication concept
- Improved data insert experience
- Mechanism to create automatically the scheduled races list

__Known bugs__

- The placing in the export files is wrong
- Race numbering / naming in scheduled race is wrong
- Deleting / Editing races is buggy
- Deleting / Editing scheduled races is buggy
- Stopwatch is not updating if started from an other client

## Prerequisite

1. ASP.NET 6 Runtime
2. MariaDB

    > __Alternative Databases__
    > - You can easily change the database to MSSQL or another technique by updating `program.cs` and `appsettings.json`

3. Clone
4. Don´t forget to create/update your database if you like to run the application.

   1. `Add-Migration Initial`
   2. `Update-Database`

## Deployment
- Currently the application does not set any cookie
- If you like to host the application please update the imprint and data protection page

