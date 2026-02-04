-- ===========================================
-- SEED DATA - Inventory Stock Items
-- ===========================================

-- Stock items for all products
INSERT INTO stock_items (product_sku, quantity, reserved_quantity) VALUES
  -- Electronics
  ('ELEC-001', 100, 0),
  ('ELEC-002', 75, 0),
  ('ELEC-003', 200, 0),
  ('ELEC-004', 50, 0),
  ('ELEC-005', 150, 0),

  -- Clothing
  ('CLTH-001', 300, 0),
  ('CLTH-002', 150, 0),
  ('CLTH-003', 80, 0),
  ('CLTH-004', 120, 0),
  ('CLTH-005', 200, 0),

  -- Home & Garden
  ('HOME-001', 40, 0),
  ('HOME-002', 100, 0),
  ('HOME-003', 90, 0),
  ('HOME-004', 60, 0),
  ('HOME-005', 30, 0),

  -- Sports & Outdoors
  ('SPRT-001', 150, 0),
  ('SPRT-002', 45, 0),
  ('SPRT-003', 35, 0),
  ('SPRT-004', 200, 0),
  ('SPRT-005', 180, 0),

  -- Books
  ('BOOK-001', 250, 0),
  ('BOOK-002', 300, 0),
  ('BOOK-003', 100, 0),
  ('BOOK-004', 150, 0),
  ('BOOK-005', 200, 0),

  -- Toys & Games
  ('TOYS-001', 80, 0),
  ('TOYS-002', 60, 0),
  ('TOYS-003', 100, 0),
  ('TOYS-004', 120, 0),
  ('TOYS-005', 90, 0),

  -- Beauty & Personal Care
  ('BEAU-001', 200, 0),
  ('BEAU-002', 150, 0),
  ('BEAU-003', 100, 0),
  ('BEAU-004', 180, 0),
  ('BEAU-005', 220, 0),

  -- Food & Beverages
  ('FOOD-001', 500, 0),
  ('FOOD-002', 400, 0),
  ('FOOD-003', 300, 0),
  ('FOOD-004', 250, 0),
  ('FOOD-005', 350, 0)
ON CONFLICT (product_sku) DO UPDATE SET
  quantity = EXCLUDED.quantity,
  reserved_quantity = 0,
  updated_at = now();
