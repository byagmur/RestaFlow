export interface task {
  id: number
  todo: string
  completed: boolean
  userId: number

}

export interface ServiceResponse {
  success: boolean
  token?: string
  message: string
  role?: string
  userId?: number
  userFName?: string
  userLName?: string
  isAuth: boolean
}


export interface OrderItem {
  id?: string | number
  productId?: string | number  // Ürün referansı
  no?: string | number        // Ürün kodu
  name: string
  price: number               // Required olmalı
  quantity: number            // Required olmalı  
  total?: number   
}

export interface Order {
  uid: string
  tableId: string | number
  orderDate: string
  totalPrice: number | string
  status: string
  waiterId: number
  note?: string
  waiterName?: string
  items?: OrderItem[]
}

export interface ProductData {
  id: string | number
  no?: string | number
  name: string
  price: number
  quantity: number
}
