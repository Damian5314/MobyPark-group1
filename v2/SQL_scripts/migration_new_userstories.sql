BEGIN;

-- 1. CREATE COMPANIES TABLE (if not exists)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_name='companies' AND table_schema='public'
    ) THEN
        CREATE TABLE companies (
            id SERIAL PRIMARY KEY,
            name TEXT NOT NULL UNIQUE,
            created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
    END IF;
END
$$;


-- 2. ADD company_id TO USERS (if not exists)

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name='Users' AND column_name='company_id'
    ) THEN
        ALTER TABLE "Users" ADD COLUMN company_id INTEGER NULL;
        ALTER TABLE "Users"
        ADD CONSTRAINT fk_users_company
        FOREIGN KEY (company_id)
        REFERENCES companies(id)
        ON DELETE SET NULL;
    END IF;
END
$$;


-- 3. INSERT COMPANIES (skip duplicates manually)

INSERT INTO companies (name)
SELECT 'Tech Solutions BV' WHERE NOT EXISTS (SELECT 1 FROM companies WHERE name='Tech Solutions BV');
INSERT INTO companies (name)
SELECT 'Logistics Pro NV' WHERE NOT EXISTS (SELECT 1 FROM companies WHERE name='Logistics Pro NV');
INSERT INTO companies (name)
SELECT 'Fleet Services Group' WHERE NOT EXISTS (SELECT 1 FROM companies WHERE name='Fleet Services Group');


-- 4. LINK USERS WITH MULTIPLE VEHICLES TO COMPANIES
-- Only users with >1 vehicle and no company_id
UPDATE "Users" u
SET company_id = (
    SELECT id FROM companies c
    ORDER BY random()
    LIMIT 1
)
WHERE u.company_id IS NULL
AND u.id IN (
    SELECT v.user_id
    FROM "Vehicles" v
    GROUP BY v.user_id
    HAVING COUNT(*) > 1
);


-- 5. CREATE HOTELS TABLE
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_name='hotels' AND table_schema='public'
    ) THEN
        CREATE TABLE hotels (
            id SERIAL PRIMARY KEY,
            name TEXT NOT NULL UNIQUE,
            created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
        );
    END IF;
END
$$;


-- 6. ADD hotel_id TO ParkingLots (if not exists)
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name='ParkingLots' AND column_name='hotel_id'
    ) THEN
        ALTER TABLE "ParkingLots" ADD COLUMN hotel_id INTEGER NULL;
        ALTER TABLE "ParkingLots"
        ADD CONSTRAINT fk_parkinglot_hotel
        FOREIGN KEY (hotel_id)
        REFERENCES hotels(id)
        ON DELETE SET NULL;
    END IF;
END
$$;


-- 7. INSERT HOTELS (skip duplicates)
INSERT INTO hotels (name)
SELECT 'Hotel Aurora' WHERE NOT EXISTS (SELECT 1 FROM hotels WHERE name='Hotel Aurora');
INSERT INTO hotels (name)
SELECT 'Grand City Hotel' WHERE NOT EXISTS (SELECT 1 FROM hotels WHERE name='Grand City Hotel');
INSERT INTO hotels (name)
SELECT 'Seaside Resort' WHERE NOT EXISTS (SELECT 1 FROM hotels WHERE name='Seaside Resort');


-- 8. LINK RANDOM PARKING LOTS TO HOTELS
-- Select 3 random ParkingLots
WITH random_parking AS (
    SELECT id FROM "ParkingLots"
    ORDER BY random()
    LIMIT 3
)
UPDATE "ParkingLots" pl
SET hotel_id = h.id,
    tariff = 0,
    daytariff = 0
FROM (
    SELECT id FROM hotels
    ORDER BY random()
    LIMIT 1
) h
WHERE pl.id IN (SELECT id FROM random_parking);

COMMIT;