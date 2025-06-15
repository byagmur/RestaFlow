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


export interface Order {
  uid: string
  tableId: string | number
  orderDate: string
  totalPrice: number | string
  status: string
  waiterId: number
}
