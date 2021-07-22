# DNUserViewer

DNUserViewer is a simple query tool to see information about a user on an Active Directory server
![DNUserViewer_8Jmh9oYsob](https://user-images.githubusercontent.com/17061996/126651574-040beb3a-4d84-4410-94d4-65a39a576f84.png)

It also allow to export the data as a json file.

DNUserViewer tries to autoconvert data, for instance the LDAP timestamps are converted to localtime ( but the raw value is saved in a field of the same name with `_RAW` appended), it also converts normal GMT datetimes to localtime (Whilst still keeping the original value id a `_GMT` field)

# License
This little tool is licensed under MIT see the [LICENSE](https://github.com/TheStaticTurtle/DNUserViewer/blob/master/LICENSE) file
