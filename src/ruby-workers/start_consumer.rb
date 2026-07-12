require 'bunny'
require 'json'
require_relative 'workers/order_confirmation_worker'

conn = Bunny.new(hostname: 'localhost')
conn.start
ch = conn.create_channel
q  = ch.queue('order_queue', auto_delete: false)

puts ' [*] Waiting for orders.'

q.subscribe(block: true) do |_delivery_info, _properties, body|
  OrderConfirmationWorker.perform_async(body)
end