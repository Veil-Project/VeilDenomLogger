# VeilDenomLogger
Performs calculation on Veil zerocoin denom data then uploads the results to veil-stats.com

## Setup
- Create a Microsoft SQL database
- Create the schema, tables and store procs using the scripts in the SqlScripts folder
- Update the app.config with the database connection string
- Also in the app.config set the ftp url, username and password to upload the json data files to veil-stats server. 
