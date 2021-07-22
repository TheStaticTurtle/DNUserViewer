# DNUserViewer

DNUserViewer is a simple query tool to see information about a user on an Active Directory server
<img src="https://i.ibb.co/HT79gnn/DNUser-Viewer-8-Jmh9o-Ysob.png" alt="DNUser-Viewer-8-Jmh9o-Ysob" border="0">

It also allow to export the data as a json file.

DNUserViewer tries to autoconvert data, for instance the LDAP timestamps are converted to localtime ( but the raw value is saved in a field of the same name with `_RAW` appended), it also converts normal GMT datetimes to localtime (Whilst still keeping the original value id a `_GMT` field)
