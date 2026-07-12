require 'sidekiq'
require 'bunny'
require 'mail'

class OrderConfirmationWorker
  include Sidekiq::Worker

  def perform(order_json)
    order = JSON.parse(order_json)
    Mail.deliver do
      from    'noreply@shop.com'
      to      "#{order['customer_id']}@example.com"
      subject 'Order Confirmation'
      body    "Your order #{order['order_id']} has been placed."
    end
    puts "Email sent for order #{order['order_id']}"
  end
end