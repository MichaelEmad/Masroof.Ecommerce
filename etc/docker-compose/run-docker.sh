#!/bin/bash

if [[ ! -d certs ]]
then
    mkdir certs
    cd certs/
    if [[ ! -f localhost.pfx ]]
    then
        dotnet dev-certs https -v -ep localhost.pfx -p 213ef8e0-7b70-4dd0-845a-e26ec735aebd -t
    fi
    cd ../
fi

docker-compose up -d
