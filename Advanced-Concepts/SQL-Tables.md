---
layout: page
title: SQL Tables
---
{% include JB/setup %}

### SQL System Storage

Any reliable production-style Orleans deployment requires using persistent storage to keep system state, specifically Orleans cluster status and the data used for the reminders functionality. In addition to out of the box support for Azure storage Orleans also provides an option to store this information in SQL server.

In order to use SQL server for system store, one needs to adjust server-side and client-side configurations.

The server configuration should look like this:

``` xml
<OrleansConfiguration xmlns="urn:orleans">
  <Globals>
      <SystemStore SystemStoreType ="SqlServer" 
                 DeploymentId="..." 
                 DataConnectionString="..." />
  </Globals>
</OrleansConfiguration>
```

The client configuration should look like this:

``` xml
<ClientConfiguration xmlns="urn:orleans">
      <SystemStore SystemStoreType ="SqlServer" 
                 DeploymentId="..." 
                 DataConnectionString="..." />
</ClientConfiguration>
```

Where the DataConnectionString is set to any valid SQL Server connection string. In order to use SQL Server as the store for system data, there’s now a script file MembershipTableCreate.sql in the Binaries\OrleansServer folder which establishes the necessary tables with the right schema. Make sure that all servers that will be hosting Orleans silos can reach the database and has access rights to it! We’ve tripped up a few times on this seemingly trivial concern, during our testing.

### SQL Metrics and Statistics tables

System tables can currently  only be stored in Azure table or SQL server.
For Metrics and Statistics tables however we provide a generic support to host it in any persistent storage. THis is provided via the notion of StatisticsProvider. Application can write an arbitrary provider to store statistics and metrcis tables in any persistent storage. Orleans provides an implemention of one such provider: SQL Table Statistics Provider.

In order to use SQL server for statistics and metrics tables, one needs to adjust server-side and client-side configurations.

The server configuration should look like this:

``` xml
<OrleansConfiguration xmlns="urn:orleans">
   <Globals>
    <StatisticsProviders>
       <Provider Type="Orleans.Providers.SqlServer.SqlStatisticsPublisher" Name="SQL" ConnectionString="..." />
     </StatisticsProviders>
   </Globals>
</OrleansConfiguration>
```

The client configuration should look like this:

``` xml
<ClientConfiguration xmlns="urn:orleans">
   <StatisticsProviders>
       <Provider Type="Orleans.Providers.SqlServer.SqlStatisticsPublisher" Name="SQL" ConnectionString="..." />
    </StatisticsProviders>
</ClientConfiguration>
```