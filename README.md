Entity Framework 6.1 Enum Lookup Table Generator
================================================

Makes up for a missing feature in Entity Framework 6.1

Creates lookup tables and foreign key constraints based on the enums
used in your model.

Use the properties exposed to control behaviour.

Run EnumToLookup.Apply from your Seed method in either your database initializer
or your EF Migrations.

It is safe to run repeatedly, and will ensure enum values are kept in line
with your current code.

For example usage (outside of a seed method) see [doc/EnumExample.cs](doc/EnumExample.cs)

Source code: https://github.com/timabell/ef-enum-to-lookup

License: MIT
