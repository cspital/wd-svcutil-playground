#! /bin/bash

xscgen -o UWD.Lib/ -n "|Resource_Management.xsd=UWD.Lib" --order Resource_Management.xsd

dotnet-svcutil Resource_Management.wsdl --outputDir Service --serializer XmlSerializer --projectFile UWD.Lib/UWD.Lib.csproj --namespace "*,UWD.Lib" --reference UWD.Lib/UWD.Lib.csproj