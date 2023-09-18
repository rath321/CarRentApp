Steps to run the project :

1. create a database named "ECommerce" {set ur connection string}
2. then run the sql files
   Note : first run the schema file, then run the Data file

3. open the sln file inside the Car_Rent.Api folder (this will automatically open the Api project in Visual studio),
   then in program manager console :
   a) run 'add-migration initial-database'
   b) run 'update-database'
   then run it.

4. then open CarRentAppUI folder in vscode :

- run "npm i"
- run "ng serve"
