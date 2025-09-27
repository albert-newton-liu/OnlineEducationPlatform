

run postgres

docker run --name my-postgres \
-e POSTGRES_PASSWORD=mysecretpassword \
-e POSTGRES_USER=postgres \
-e POSTGRES_DB=OnlineEducation \
-p 5432:5432 \
-v pgdata:/var/lib/postgresql/data \
-d postgres:latest

dotnet ef migrations add init

dotnet ef database update
