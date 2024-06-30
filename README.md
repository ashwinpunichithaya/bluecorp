# Bluecorp Inventory Automation

This repository contains sample source code to enable inventory automation for Bluecorp. 

## Proposed Design:
The solution leverages Azure Integration Sevices to automate order fulfillment for Bluecorp. The solution has 3 main components:
1. Dispatch Order HTTP Trigger Azure Function - This captures JSON payload from D365. When the user clicks on `Ready to Dispatch` button from D365 screen, collated order data is posted to Azure Function which does some basic validation before forwarding on the JSON payload to Azure Service Bus Queue. If request is missing payload, or payload does not conform to defined JSON schema consumers will be returned HTTP 400 Bad Request response. If request is valid, then Dispatch Order Function forwards the JSON payload to Azure Service BUS Queue with 'controlNumber' attribute mapped to service bus message id. This helps filter out any duplicate or repeat JSON payloads posted from D365.
2. Azure Service Bus Queue: Landing area for all the JSON payloads posted from D365.
3. Process Order Service Bus Queue Trigger Azure Function - This function gets triggered as new JSON payloads arrive in the queue. The purpose of this function is to tranform JSON payload to CSV and publish it to 3PL SFTP store for order fulfillment. If the function is able to successfully process incoming JSON dispatch order, then correponding message is taken off the queue, else the message remains in queue for until next try.

### Authentication
1. D365 to Dispatch Order Azure Function - Dispatch Order Azure Function will be configured for RBAC. New app registration will be made available with `Contributor` rights on the function. D365 app is expected to use the EntraID credentials (Client and Secret) to invoke the function.
2. Managed Identities: The functions, service bus and associated storage accounts will leverage managed identities (System or User) to communicate with each other. The identities will be assigned necessary roles to enable the communication. For e.g. Dispatch Order Azure Function will be assigned `Azure Service Bus Data Sender` to allow the JSON payloads to be sent to the service bus queue. The role assignments will mirror operations supported by the function.

### Secrets Management
1. All the secrets be it ConnectionStrings or Access Keys will be managed via Azure Key Vault
   
### App Insights
1. App Insights telemetry will be enabled on all the azure services.
2. Any custom telemetry will be ingested using TelemetryClient.
   
![image](https://github.com/ashwinpunichithaya/bluecorp/assets/61331734/72fb85a8-1767-40e6-a2ee-0acb1e583218)

## Alternative Design
The proposed solution uses code mapping to translate JSON payload to CSV. This is a drawback as any minor change in mapping would require us to rebuild the functions. Azure Data Factory is good alternative to overcome the drawback in the proposed design. The author has not worked on Azure Data Factory and hence the proposed design. 

### Tweaks to the proposed design
1. Dispatch Function to send events to Azure Blob Storage
2. Azure Data Factory to read incoming events off blob storage and use data flows (Flatten transformation, Get Metadata Activity etc.) to map and transform JSON to CSV.
3. Use Azure Data Factory SFTP connector push CSV to 3PL SFTP store.

## Sample Code TODOs

1. Custom Telemetry
2. JSON schema validation for incoming payload
3. Reading secrets from Azure Key Vault
4. HTTPS/TLS communication
5. App configuration driven ContainerType mapping
6. Azure Pipeline for build and deployment enabled for different environment settings like 'DEV', 'TEST', 'PREPROD' and 'PROD'
7. OAuth 2.0 EntraID based authentication for Dispatch Order Function
8. OpenAPI/Swagger documentation
9. Unit tests
10. Automated integration testing facilitated by Postman
11. Extend proposed/alternative design to support auditing of incoming requests using stores like Azure CosmosDb
12. Performance testing
13. And more...
