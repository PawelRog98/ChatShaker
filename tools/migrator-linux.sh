#!/bin/bash

migratorLoc="/home/parroter/Repos/ChatShaker/ChatShaker.Migrator"
dbconn="Server=127.0.0.1,1433;Database=ChatShakerTest;User Id=sa;Password=Password-1;TrustServerCertificate=True;MultipleActiveResultSets=true;"

clear

echo "Choose option to run:"
echo "1) Migration"
echo "2) Rollback number of migrations"
echo "3) Rollback all migrations"

read -p "Choose option (1-3): " choice

case "$choice" in
    1)
        command="migrate"
        ;;
    2)
        read -p "Set quantity of migrations to rollback: " rollback_quantity
        if [ -z "$rollback_quantity" ]; then
            echo "Error"
            exit 1
        fi
        command="rollback $rollback_quantity"
        ;;
    3)
        command="rollbackall"
        ;;
esac

cd "$migratorLoc"

dotnet run -- "$dbconn" "$command"

read -p "Press Enter to exit..."
exit 1