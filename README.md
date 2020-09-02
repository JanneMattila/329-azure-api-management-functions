# 329-Azure API Management with Functions

If you need to create APIs to API Management  but you currently have data only available
as static csv files, you can use following architecture to start exposing
them as APIs:

![architecture](https://user-images.githubusercontent.com/2357647/91958498-1a9d5680-ed10-11ea-8bc4-be5d63564033.png)

The *glue* that is between API Management is Azure Functions app _Csv Converter_ that is
responsible of fetching the csv file and corresponding mapping file. It then uses
information from mapping file to convert csv data to json response. 

If you need to provided additional conversion logic to _Csv Converter_ then you can
enhance mapping file to do that.

Here is example csv:

```csv
ID,Name,Description
1,Car,This is description of car
2,Bicycle,It has two wheels
3,House,It's large building
```

Here is example map:

```json
{
  "delimiter": ",",
  "useQuotes": false,
  "fieldMappings": [
    {
      "name": "ID",
      "type": "integer"
    },
    {
      "name": "Name",
      "type": "string"
    },
    {
      "name": "Description",
      "type": "string"
    }
  ]
}
```

If you now invoke function API:

```http
### Get csv data using map
POST {{func}}/api/csv HTTP/1.1
Content-type: application/json

{
  "csv": "https://....blob.core.windows.net/files/data.csv",
  "map": "https://....blob.core.windows.net/files/data_map.json"
}
```

You would get following response:

```json
[
  {
    "ID": 1,
    "Name": "Car",
    "Description": "This is description of car"
  },
  {
    "ID": 2,
    "Name": "Bicycle",
    "Description": "It has two wheels"
  },
  {
    "ID": 3,
    "Name": "House",
    "Description": "It's large building"
  }
]
```

Now you can re-use this same function app in your different APIs.
