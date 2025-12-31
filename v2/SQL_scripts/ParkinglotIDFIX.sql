SELECT setval(
    pg_get_serial_sequence('"ParkingLots"', 'Id'),
    COALESCE(MAX("Id"), 1)
)
FROM "ParkingLots";