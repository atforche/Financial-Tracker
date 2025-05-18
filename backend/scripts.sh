run-full-pipeline()
{
    restore-solution
    format-solution
    build-solution
    run-unit-tests
}

restore-solution()
{
    echo "Restoring solution"
    dotnet restore Backend.sln
    echo -e "Restore completed\n"
}

format-solution()
{
    echo "Formatting solution"
    dotnet format Backend.sln --verify-no-changes --no-restore --severity info
    echo -e "Format completed\n"
}

build-solution()
{
    echo "Building solution"
    dotnet build Backend.sln --no-restore
    echo -e "Build completed\n"
}

run-unit-tests()
{
    echo "Running unit tests"
    dotnet test ./Backend.sln --no-build --verbosity normal
    echo -e "Unit tests completed\n"
}

run-database-integration-tests()
{
    echo "Running database integration tests"
    dotnet test ./Backend.sln --no-build --verbosity normal --environment USE_DATABASE=TRUE
    echo -e "Database integration tests completed\n" 
}