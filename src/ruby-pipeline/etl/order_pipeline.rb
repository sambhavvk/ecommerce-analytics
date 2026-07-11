require 'kiba'
require 'kiba-common/sources/sql'
require 'click_house'

# Define ClickHouse connection
ClickHouse.config do |config|
  config.adapter  = :net_http
  config.url      = 'http://localhost:8123'
  config.database = 'analytics'
end

# Source: PostgreSQL orders
source Kiba::Common::Sources::SQL, connection_string: "postgres://admin:secret@localhost:5432/orders",
       query: "SELECT id, customer_id, status, placed_at FROM orders"

# Transform: normalize
transform do |row|
  row[:status] = row[:status].downcase
  row
end

# Destination: ClickHouse
destination ->(rows) {
  rows.each do |row|
    ClickHouse.connection.insert_row('orders',
      values: [row[:id], row[:customer_id], row[:status], row[:placed_at].to_s])
  end
}