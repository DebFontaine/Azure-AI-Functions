# Azure AI Functions

This is a hobby app to play around with Azure Functions and Azure AI Services (Image and Text). The app consists of an Angular Front End which uses PrimeNG UI Components, a .NET Core Web API for authorization and integration with Google Drive API, and 3 Azure Functions that integrate with Azure AI Services.

![enter image description here](https://res.cloudinary.com/dngjhgdql/image/upload/v1710552419/Screen_Shot_2024-03-15_at_9.25.27_PM_srlt0w.png)


# Architecture
![enter image description here](https://res.cloudinary.com/dngjhgdql/image/upload/v1710594061/Azure_AI_Functions_1_lmlkw8.png) 


# Current Features

**Dashboard Page** - The main dashboard page displays 3 Widgets that allow users to analyze image and text files using Azure AI

Analyze Image - uses Azure AI Vision to extract keywords from an uploaded image

Analyze Text - Uses Azure AI Text to extract key phrases from a text document

Summarize Text - Uses Azure AI Text to summarize a longer text document

**Assets Page** - If a user is logged in they get a view of their folder\file assests in the google drive directory associated with their account. The user can toggle a switch to use Azure Vision AI to extract the tags from an image and add up to 3 to the image they are uploading. The file is then searchable via the tags.

Analyze image:

![enter image description here](https://res.cloudinary.com/dngjhgdql/image/upload/v1710551638/Screen_Shot_2024-03-15_at_5.37.43_PM_tztcqs.png)

Select tags and continue upload:

![enter image description here](https://res.cloudinary.com/dngjhgdql/image/upload/v1710553483/Screen_Shot_2024-03-15_at_9.41.47_PM_ttzxrl.png)

Search via added tags:

![enter image description here](https://res.cloudinary.com/dngjhgdql/image/upload/v1710553482/Screen_Shot_2024-03-15_at_9.44.23_PM_jvj9ar.png)




