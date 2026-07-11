require 'sinatra'
require 'sinatra/reloader' if development?
require 'sequel'
require 'json'

DB = Sequel.connect('postgres://admin:secret@localhost:5432/orders')

before do
  headers 'Content-Type' => 'text/html; charset=utf-8'
end

get '/' do
  @orders = DB[:orders].order(:placed_at).limit(50).all
  erb :index
end

get '/orders/:id' do
  @order = DB[:orders].where(id: params[:id]).first
  erb :show
end

post '/orders/:id/ship' do
  @order = DB[:orders].where(id: params[:id]).first
  DB[:orders].where(id: params[:id]).update(status: 'shipped')
  @order[:status] = 'shipped'

  content_type 'text/vnd.turbo-stream.html'
  erb :_order_status, layout: false
end