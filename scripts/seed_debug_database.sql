insert into Funds (Id, Name, Description)
values
    ('CCDD9EB7-1551-4808-9059-DDD46E9CBFF0', 'Spending', 'This is the spending fund'),
    ('E0FBA450-5CE3-4ADE-863B-37D5490E056C', 'Reserve', 'This is the reserve fund'),
    ('9ACF9A73-897B-4680-B2B1-51E801600511', 'Savings', 'This is the savings fund'),
    ('5ED7EF9F-92F2-46D5-9CA1-718B92EC18C0', 'Safety Net', 'This is the safety net fund'),
    ('F1B746CE-1422-4EFE-97DE-93F8C03B6BB3', 'Retirement', 'This is the retirement fund'),
    ('A03CAA79-8600-4EB4-842B-7D0E5E7B4876', 'Debt', 'This is the debt fund');

insert into AccountingPeriods (Id, Year, Month, IsOpen)
values    
    ('EA26EA49-876F-4E97-A9C0-661737EFAC5C', 2026, 1, false),
    ('95033FED-2BE0-4A7B-A9FC-F359C072AD4D', 2026, 2, true);
