-- enum tanımı
do $$
begin
    if not exists (select 1 from pg_type where typname = 'reservation_status') then
        create type reservation_status as enum ('pending', 'confirmed', 'released', 'cancelled');
    end if;
end
$$;

-- stock_items tablosu
create table if not exists stock_items (
    id                  bigserial primary key,
    product_sku         text unique not null,
    quantity            integer not null check (quantity >= 0),
    reserved_quantity   integer not null default 0 check (reserved_quantity >= 0),
    updated_at          timestamptz not null default now()
);

-- reservations tablosu
create table if not exists reservations (
    id              bigserial primary key,
    order_id        bigint not null,
    product_sku     text not null,
    quantity        integer not null check (quantity > 0),
    status          reservation_status not null default 'pending',
    expires_at      timestamptz,
    created_at      timestamptz not null default now()
);

-- indexler
create index if not exists idx_stock_items_product
    on stock_items(product_sku);

create index if not exists idx_reservations_order
    on reservations(order_id);

create index if not exists idx_reservations_status
    on reservations(status);