run-full-pipeline()
{
    restore-solution
    format-solution
    build-solution
    test-solution
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

test-solution()
{
    echo "Testing solution"
    dotnet test ./Backend.sln --no-build --verbosity normal
    echo -e "Testing completed\n"
}