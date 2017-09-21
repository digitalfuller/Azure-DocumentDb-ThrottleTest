# AZURE-DOCUMENTDB-THROTTLETEST

This console application attempts to hit the document db throttling limit by firing a number of parallel tasks at a single collection.

To use this application you'll need to update the following settings in the app.config to valid Azure document db settings.

``` xml
<appSettings>

    <add key="DatabaseServiceEndpoint" value="YOUR-DATABASE-SERVICE-ENDPOINT"/>

    <add key="DatabaseId" value="YOUR-DATABASE-ID"/>

    <add key="DatabaseAuthKey" value="YOUR-DATABASE-AUTH-KEY"/>

    <add key="DatabaseCollectionName" value="YOUR-COLLECTION-NAME"/>

    <add key="MaxPendingTasks" value="10"/>

  </appSettings>
  ```