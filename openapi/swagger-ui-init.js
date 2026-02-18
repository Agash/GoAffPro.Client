
window.onload = function() {
  // Build a system
  var url = window.location.search.match(/url=([^&]+)/);
  if (url && url.length > 1) {
    url = decodeURIComponent(url[1]);
  } else {
    url = window.location.origin;
  }
  var options = {
  "swaggerDoc": {
    "openapi": "3.0.0",
    "info": {
      "description": "API to interact with goaffpro app",
      "version": "1.0.0",
      "title": "GOAFFPRO API",
      "termsOfService": "http://goaffpro.com/terms/",
      "contact": {
        "email": "admin@goaffpro"
      },
      "license": {
        "name": "Apache 2.0",
        "url": "http://www.apache.org/licenses/LICENSE-2.0.html"
      }
    },
    "servers": [
      {
        "url": "https://api.goaffpro.com/v1/"
      }
    ],
    "tags": [
      {
        "name": "affiliate",
        "description": "Admin access to affiliates on your program",
        "externalDocs": {
          "description": "Find out more",
          "url": "http://docs.goaffpro.com"
        }
      },
      {
        "name": "orders",
        "description": "Admin access to referral orders of the affiliate"
      },
      {
        "name": "rewards",
        "description": "Admin access to rewards given to the affiliates (eg. Signup bonus, Target bonus, MLM Commission etc.)"
      },
      {
        "name": "payments",
        "description": "Admin access to payments made to the affiliates",
        "externalDocs": {
          "description": "Find out more",
          "url": "http://docs.goaffpro.com"
        }
      },
      {
        "name": "user",
        "description": "API Endpoints for affiliates to use"
      },
      {
        "name": "commissions",
        "description": "Access to commission rate setup on products and collections"
      },
      {
        "name": "multi-level",
        "description": "Access to the multi-level marketing",
        "externalDocs": {
          "description": "Find out more",
          "url": "http://docs.goaffpro.com"
        }
      },
      {
        "name": "connections",
        "description": "Connections between affiliate and customers for giving perpetual commission to the affiliate for the customer's orders",
        "externalDocs": {
          "description": "Find out more",
          "url": "http://docs.goaffpro.com"
        }
      },
      {
        "name": "creatives",
        "description": "Access to media assets uploaded in the admin panel"
      },
      {
        "name": "files",
        "description": "Access to files uploaded by the affiliates in their dashboard"
      },
      {
        "name": "groups",
        "description": "Create groups which share common configuration. An affiliate can be put into a group. An affiliate inside a group follows the configuration defined in the group instead of standard app configuration"
      },
      {
        "name": "traffic",
        "description": "Access to affiliate traffic data"
      },
      {
        "name": "config",
        "description": "Access to store configuration"
      },
      {
        "name": "sdk",
        "description": "Access to public endpoints in the system (requires x-goaffpro-public-token in the header)"
      },
      {
        "name": "public",
        "description": "Access to public data set of the merchants enrolled in the marketplace program"
      },
      {
        "name": "webhooks",
        "description": "Webhooks to receive events from the system"
      },
      {
        "name": "transactions",
        "description": "Access to transaction log entries"
      }
    ],
    "schemes": [
      "https"
    ],
    "components": {
      "securitySchemes": {
        "admin": {
          "type": "apiKey",
          "name": "x-goaffpro-access-token",
          "description": "Access token for the store obtained from the goaffpro app. You can generate new Access Tokens in Settings -> Advanced Tab -> API Keys section",
          "in": "header"
        },
        "user": {
          "type": "http",
          "scheme": "bearer",
          "bearerFormat": "JWT",
          "in": "header",
          "description": "The affiliate access token. Used in the /user/ and /sdk/user endpoints. Generate this by using the /user/login (or /sdk/user/login) endpoint"
        },
        "userAdmin": {
          "type": "apiKey",
          "name": "x-goaffpro-access-token",
          "description": "Access token for obtained from your affiliate dashboard. You can generate new Access Tokens in Settings -> Access token section of your dashboard. You can use any one of the authentication strategies",
          "in": "header"
        },
        "sdk": {
          "type": "apiKey",
          "name": "x-goaffpro-public-token",
          "in": "header",
          "description": "Access token to call the /sdk/ endpoints. The SDK endpoints have CORS enabled and can be called in public scope from your storefront (or any webpage)"
        }
      },
      "schemas": {
        "Affiliate": {
          "type": "object",
          "properties": {
            "id": {
              "type": "integer",
              "description": "Affiliate id of the affiliate"
            },
            "name": {
              "type": "string"
            },
            "first_name": {
              "type": "string"
            },
            "last_name": {
              "type": "string"
            },
            "facebook": {
              "type": "string"
            },
            "twitter": {
              "type": "string"
            },
            "instagram": {
              "type": "string"
            },
            "website": {
              "type": "string"
            },
            "ref_code": {
              "type": "string"
            },
            "coupon": {
              "type": "string"
            },
            "phone": {
              "type": "string"
            },
            "address_1": {
              "type": "string"
            },
            "city": {
              "type": "string"
            },
            "state": {
              "type": "string"
            },
            "country": {
              "type": "string"
            },
            "zip_code": {
              "type": "string"
            },
            "comments": {
              "type": "string"
            },
            "personal_message": {
              "type": "string"
            },
            "gender": {
              "type": "string"
            },
            "honorific": {
              "type": "string"
            },
            "date_of_birth": {
              "type": "string"
            },
            "group_id": {
              "type": "integer",
              "description": "The group the affiliate is part of. If the affiliate is not a part of any group, this field is empty"
            },
            "ref_codes": {
              "type": "array",
              "items": {
                "type": "string"
              },
              "description": "List of referral codes assigned to the affiliate"
            },
            "coupons": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "code": {
                    "type": "string",
                    "description": "Coupon code assigned to the affiliate"
                  },
                  "discount_type": {
                    "type": "string",
                    "enum": [
                      "percentage",
                      "fixed_amount"
                    ],
                    "description": "Type of discount given by this coupon code"
                  },
                  "discount_value": {
                    "type": "number",
                    "description": "The amount of discount given"
                  }
                }
              }
            },
            "tin": {
              "type": "string",
              "description": "Tax Identification Number"
            },
            "payment_method": {
              "type": "string"
            },
            "payment_details": {
              "type": "object",
              "properties": {}
            },
            "extra_1": {
              "type": "string"
            },
            "extra_2": {
              "type": "string"
            },
            "extra_3": {
              "type": "string"
            },
            "registration_ip": {
              "type": "string"
            },
            "commission": {
              "type": "object",
              "properties": {
                "type": {
                  "type": "string",
                  "enum": [
                    "percentage",
                    "fixed_amount",
                    "fixed_amount_on_order"
                  ]
                },
                "amount": {
                  "type": "integer"
                },
                "on": {
                  "type": "string",
                  "enum": [
                    "product",
                    "order"
                  ],
                  "description": "The commission to be given on. Useful if you wish to give flat rate commission on the entire order instead of giving commission on a per product basis"
                }
              }
            }
          },
          "example": {
            "id": 1,
            "name": "John Doe",
            "avatar": {},
            "first_name": "John",
            "last_name": "Doe",
            "email": "johndoe@example.com",
            "ref_code": "hs692n62d",
            "ref_codes": [
              "referralcode"
            ],
            "coupon": {
              "code": "JOHN10OFF",
              "discount_value": 10,
              "discount_type": "percentage"
            },
            "coupons": [
              {
                "code": "JOHN10OFF",
                "discount_value": 10,
                "discount_type": "percentage"
              }
            ],
            "phone": "1 888 (999) 1234",
            "address_1": "Address line 1",
            "city": "City",
            "state": "State",
            "country": "US",
            "zip_code": "121323232",
            "tax_identification_number": "788-21-1122",
            "comments": "Private comment",
            "personal_message": "Message to the affiliate",
            "payment_method": "paypal",
            "payment_details": {
              "paypal_email": "jdoe@paypal.com"
            },
            "commission": {
              "type": "percentage",
              "amount": 10,
              "on": "product"
            },
            "group_id": 10
          }
        },
        "Connections": {
          "type": "object",
          "properties": {
            "id": {
              "type": "integer",
              "description": "Connection ID"
            },
            "affiliate": {
              "type": "object",
              "properties": {
                "id": {
                  "type": "integer",
                  "string": "ID of the affiliate"
                }
              }
            },
            "customer": {
              "type": "object",
              "properties": {
                "name": {
                  "type": "string",
                  "string": "Name of the customer"
                },
                "email": {
                  "type": "string",
                  "string": "Email address of the customer"
                }
              }
            },
            "created_at": {
              "type": "string",
              "description": "Date when this connection was created"
            }
          },
          "example": {
            "connection_id": 1,
            "affiliate": {
              "id": 42
            },
            "customer": {
              "name": "John Doe",
              "email": "johndoe@example.com"
            },
            "created_at": "Date Sat Jul 25 2020 12:31:01 GMT+0530 (India Standard Time)"
          }
        },
        "PaymentRequest": {
          "type": "object",
          "properties": {
            "id": {
              "type": "integer",
              "description": "Payment request ID"
            },
            "affiliate_id": {
              "type": "integer"
            },
            "amount": {
              "type": "integer",
              "description": "The amount requested by the affiliate"
            },
            "note": {
              "type": "string"
            },
            "invoice": {
              "type": "object",
              "properties": {
                "url": {
                  "type": "string",
                  "description": "URL to download the invoice"
                }
              }
            },
            "status": {
              "type": "string",
              "enum": [
                "in_progress",
                "paid",
                "rejected"
              ]
            },
            "created_at": {
              "type": "string"
            },
            "updated_at": {
              "type": "string"
            }
          },
          "example": {
            "id": 1,
            "affiliate_id": "John Doe",
            "amount": "10.99",
            "note": "Note from the affiliate",
            "invoice": {
              "url": "https://static.goaffpro.com/32/223323.pdf"
            },
            "created_at": "Wed Feb 18 2026 07:38:43 GMT+0100 (Central European Standard Time)",
            "updated_at": "Wed Feb 18 2026 07:38:43 GMT+0100 (Central European Standard Time)"
          }
        },
        "Order": {
          "type": "object",
          "properties": {
            "id": {
              "type": "integer",
              "description": "Order ID"
            },
            "total": {
              "type": "number",
              "description": "Order total as in order receipt"
            },
            "subtotal": {
              "type": "number",
              "description": "The amount on which commission is calculated"
            },
            "affiliate_id": {
              "type": "number",
              "description": "ID of the affiliate who brought the order"
            },
            "commission": {
              "type": "number",
              "description": "Commission given to affiliate for this order"
            },
            "status": {
              "type": "string",
              "enum": [
                "approved",
                "rejected"
              ]
            },
            "data": {
              "type": "object",
              "description": "RAW ORDER DATA. Only for advanced use cases"
            },
            "created": {
              "type": "string",
              "description": "The date when order was created"
            }
          },
          "example": {
            "id": 1,
            "affiliate_id": 1,
            "total": 100,
            "subtotal": 90,
            "commission": 10,
            "status": "approved",
            "created": "Wed Feb 18 2026 07:38:43 GMT+0100 (Central European Standard Time)"
          }
        },
        "Rewards": {
          "type": "object",
          "properties": {
            "id": {
              "type": "integer",
              "description": "Order ID"
            },
            "affiliate_id": {
              "type": "number",
              "description": "ID of the affiliate who brought the order"
            },
            "type": {
              "type": "string",
              "enum": [
                "signup_bonus",
                "sale_commission",
                "target_bonus",
                "wallet_adjustment",
                "recruitment_bonus"
              ]
            },
            "metadata": {
              "type": "string"
            },
            "order_id": {
              "type": "number"
            },
            "level": {
              "type": "number"
            },
            "amount": {
              "type": "integer",
              "description": "Reward amount"
            },
            "status": {
              "type": "string",
              "enum": [
                "approved",
                "rejected"
              ]
            },
            "created": {
              "type": "string",
              "description": "The date when order was created"
            }
          },
          "example": {
            "id": 1,
            "affiliate_id": 1,
            "amount": 100,
            "status": "approved",
            "type": "signup_bonus",
            "created": "Wed Feb 18 2026 07:38:43 GMT+0100 (Central European Standard Time)",
            "order_id": 2342233,
            "level": 1
          }
        },
        "OrderInput": {
          "type": "object",
          "properties": {
            "id": {
              "type": "integer",
              "description": "Affiliate id of the affiliate"
            },
            "name": {
              "type": "string"
            },
            "first_name": {
              "type": "string"
            },
            "last_name": {
              "type": "string"
            },
            "facebook": {
              "type": "string"
            },
            "twitter": {
              "type": "string"
            },
            "instagram": {
              "type": "string"
            },
            "website": {
              "type": "string"
            },
            "ref_code": {
              "type": "string"
            },
            "coupon": {
              "type": "string"
            },
            "phone": {
              "type": "string"
            },
            "address_1": {
              "type": "string"
            },
            "city": {
              "type": "string"
            },
            "state": {
              "type": "string"
            },
            "country": {
              "type": "string"
            },
            "zip_code": {
              "type": "string"
            },
            "comments": {
              "type": "string"
            },
            "personal_message": {
              "type": "string"
            },
            "group_id": {
              "type": "integer",
              "description": "The group the affiliate is part of. If the affiliate is not a part of any group, this field is empty"
            },
            "ref_codes": {
              "type": "array",
              "items": {
                "type": "string"
              },
              "description": "List of referral codes assigned to the affiliate"
            },
            "coupons": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "code": {
                    "type": "string",
                    "description": "Coupon code assigned to the affiliate"
                  },
                  "discount_type": {
                    "type": "string",
                    "enum": [
                      "percentage",
                      "fixed_amount"
                    ],
                    "description": "Type of discount given by this coupon code"
                  },
                  "discount_value": {
                    "type": "number",
                    "description": "The amount of discount given"
                  }
                }
              }
            },
            "tin": {
              "type": "string",
              "description": "Tax Identification Number"
            },
            "payment_method": {
              "type": "string"
            },
            "payment_details": {
              "type": "object",
              "properties": {}
            },
            "extra_1": {
              "type": "string"
            },
            "extra_2": {
              "type": "string"
            },
            "extra_3": {
              "type": "string"
            },
            "registration_ip": {
              "type": "string"
            },
            "commission": {
              "type": "object",
              "properties": {
                "type": {
                  "type": "string",
                  "enum": [
                    "percentage",
                    "fixed_amount",
                    "fixed_amount_on_order"
                  ]
                },
                "amount": {
                  "type": "integer"
                },
                "on": {
                  "type": "string",
                  "enum": [
                    "product",
                    "order"
                  ],
                  "description": "The commission to be given on. Useful if you wish to give flat rate commission on the entire order instead of giving commission on a per product basis"
                }
              }
            }
          },
          "example": {
            "id": 1,
            "name": "John Doe",
            "avatar": {},
            "first_name": "John",
            "last_name": "Doe",
            "email": "johndoe@example.com",
            "ref_code": "hs692n62d",
            "ref_codes": [
              "referralcode"
            ],
            "coupon": {
              "code": "JOHN10OFF",
              "discount_value": 10,
              "discount_type": "percentage"
            },
            "coupons": [
              {
                "code": "JOHN10OFF",
                "discount_value": 10,
                "discount_type": "percentage"
              }
            ],
            "phone": "1 888 (999) 1234",
            "address_1": "Address line 1",
            "city": "City",
            "state": "State",
            "country": "US",
            "zip_code": "121323232",
            "tax_identification_number": "788-21-1122",
            "comments": "Private comment",
            "personal_message": "Message to the affiliate",
            "payment_method": "paypal",
            "payment_details": {
              "paypal_email": "jdoe@paypal.com"
            },
            "commission": {
              "type": "percentage",
              "amount": 10,
              "on": "product"
            },
            "group_id": 10
          }
        }
      }
    },
    "paths": {
      "/admin/orders": {
        "get": {
          "tags": [
            "orders"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "description": "Retrieves list of orders for the affiliates.",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "query",
              "name": "id",
              "description": "Retrieve only the orders specified by a comma-separated list of order IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "number",
              "description": "Retrieve only the orders specified by a comma-separated list of order numbers",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "customer_email",
              "description": "Retrieve only the orders matching the customer email address",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "affiliate_id",
              "description": "Retrieve only the orders specified by a comma-separated list of Affiliate IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "status",
              "description": "Retrieve only orders specified by a comma-separated list of their approval status",
              "schema": {
                "type": "string",
                "enum": [
                  "new",
                  "approved",
                  "rejected"
                ]
              }
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Show orders after the specified order ID",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "created_at_max",
              "schema": {
                "type": "string"
              },
              "description": "Show orders created at or before date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "created_at_min",
              "schema": {
                "type": "string"
              },
              "description": "Show orders created at or after date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "is_new_customer",
              "schema": {
                "type": "number"
              },
              "description": "Shows orders where the customer is a new customer. 1 = new customer, 0 = old customer"
            },
            {
              "in": "query",
              "name": "is_subscription",
              "schema": {
                "type": "number"
              },
              "description": "Shows orders where order is a subscription order. 1= subscription order, 0 = not a subscription order"
            },
            {
              "in": "query",
              "name": "is_subscription_renewal",
              "schema": {
                "type": "number"
              },
              "description": "Shows orders where order is a subscription renewal. 1= subscription renewal order, 0 = not a subscription renewal order"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "affiliate_id",
                    "number",
                    "total",
                    "subtotal",
                    "commission",
                    "status",
                    "sub_id",
                    "coupons",
                    "mlm_amount",
                    "created",
                    "type",
                    "customer_email",
                    "is_new_customer",
                    "line_items",
                    "shipping_address",
                    "customer",
                    "is_paid"
                  ]
                }
              },
              "explode": false,
              "required": true
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "orders": {
                        "type": "array",
                        "items": {
                          "$ref": "#/components/schemas/Order"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "orders"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "description": "Manually add an order and commission to an affiliate. For full order schema which can be passed see https://github.com/anujtenani/goaffpro/wiki/Custom-Integration-advanced-guide. Both BASIC and EXTENDED ORDER Schemas are supported. All the fields other than the BASIC ORDER SCHEMA (i.e. number and total) are optional",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "order": {
                      "type": "object",
                      "properties": {
                        "number": {
                          "required": true,
                          "type": "string",
                          "description": "The order number to display"
                        },
                        "total": {
                          "required": true,
                          "type": "integer",
                          "description": "The total order value"
                        }
                      }
                    },
                    "affiliate_id": {
                      "required": false,
                      "type": "string",
                      "description": "ID of the affiliate to give the order to. Can be omitted if ref_code field is set"
                    },
                    "ref_code": {
                      "required": false,
                      "type": "string",
                      "description": "Referral code of the link used in this order. Can be omitted if affiliate_id field is set"
                    }
                  }
                },
                "example": {
                  "order": {
                    "id": "1001",
                    "number": "#1001",
                    "total": 1000,
                    "coupons": [
                      "EASY10OFF"
                    ]
                  },
                  "affiliate_id": 456822,
                  "ref_code": "7hbas62nd89"
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "result": {
                        "error": {
                          "type": "string"
                        },
                        "commission": {
                          "type": "integer"
                        },
                        "affiliate_id": {
                          "type": "string"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/orders/{id}": {
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "number"
            }
          }
        ],
        "patch": {
          "tags": [
            "orders"
          ],
          "summary": "Update an order",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "commission": {
                      "type": "number"
                    },
                    "status": {
                      "type": "string",
                      "enum": [
                        "pending",
                        "approved",
                        "rejected"
                      ]
                    }
                  }
                }
              },
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "commission": {
                      "type": "number"
                    },
                    "status": {
                      "type": "string",
                      "enum": [
                        "pending",
                        "approved",
                        "rejected"
                      ]
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "id": {
                        "type": "number"
                      },
                      "commission": {
                        "type": "number"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "delete": {
          "tags": [
            "orders"
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "summary": "Deletes an order and it's commission from the affiliate's account",
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "success": {
                        "type": "boolean"
                      },
                      "deleted": {
                        "type": "number",
                        "description": "ID of the order that was deleted"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/orders/recalculate/{id}": {
        "parameters": [
          {
            "name": "id",
            "in": "path",
            "required": true,
            "schema": {
              "type": "number"
            }
          }
        ],
        "post": {
          "summary": "Recalculates commission for the provided order ID",
          "tags": [
            "orders"
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "commission": {
                        "type": "number"
                      },
                      "subtotal": {
                        "type": "number"
                      },
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/orders/system": {
        "get": {
          "tags": [
            "orders"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "summary": "Retrieves list of order IDs from the system for later processing via the POST endpoint. The number of entries returned varies depending on which platform your store is using",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "orders": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "string",
                              "description": "The ID of the order in the system"
                            },
                            "number": {
                              "type": "string",
                              "description": "The order number reported in the system"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "orders"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "summary": "Queues orders for processing. You can optionally specify affiliate id force assign the order to the affiliate, otherwise the system will try and determine the correct affiliate on its own and the order is only assigned if an affiliate is found. If there are > 5 entries, the orders are queued otherwise the orders are processed immediately",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "orders": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "id": {
                            "type": "string",
                            "description": "ID of the order retrieved "
                          },
                          "affiliate_id": {
                            "type": "string"
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/rewards": {
        "get": {
          "tags": [
            "rewards"
          ],
          "summary": "List rewards give to the affiliates",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "id",
              "description": "Retrieve only the rewards specified by a comma-separated list of reward IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "affiliate_id",
              "description": "Retrieve only the rewards specified by a comma-separated list of Affiliate IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "type",
              "description": "Retrieve only the rewards specified by a comma-separated list of Reward types",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "status",
              "description": "Retrieve only rewards specified by a comma-separated list of their approval status",
              "schema": {
                "type": "string",
                "enum": [
                  "approved",
                  "rejected"
                ]
              }
            },
            {
              "in": "query",
              "name": "order_id",
              "description": "Retrieve only rewards specified by a comma-separated list of order IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Show rewards after the specified reward ID",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "created_at_max",
              "schema": {
                "type": "string"
              },
              "description": "Show rewards created at or before date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "created_at_min",
              "schema": {
                "type": "string"
              },
              "description": "Show rewards created at or after date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return. See Rewards Schema to get list of available fields",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "affiliate_id",
                    "amount",
                    "metadata",
                    "status",
                    "created_at",
                    "updated_at",
                    "level",
                    "order_id"
                  ]
                }
              },
              "explode": false,
              "required": true
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "rewards": {
                        "type": "array",
                        "items": {
                          "$ref": "#/components/schemas/Rewards"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "rewards"
          ],
          "summary": "Give rewards to the affiliates",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "rewards": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "amount": {
                            "type": "integer",
                            "required": true,
                            "description": "Reward amount to give to the affiliate"
                          },
                          "type": {
                            "type": "string",
                            "enum": [
                              "signup_bonus",
                              "sale_commission",
                              "recruitment_bonus",
                              "target_bonus"
                            ],
                            "required": true
                          },
                          "metadata": {
                            "type": "string",
                            "description": "Any piece of metadata you wish to attach with this entry"
                          },
                          "order_id": {
                            "type": "string",
                            "description": "Required for reward type of sale_commission. The order id for whom this upline level commission is given"
                          },
                          "level": {
                            "type": "string",
                            "description": "Optional. Only valid for reward type of sale_commission. The upline level number"
                          },
                          "status": {
                            "type": "string",
                            "enum": [
                              "pending",
                              "approved",
                              "rejected"
                            ]
                          }
                        }
                      }
                    }
                  },
                  "example": {
                    "rewards": [
                      {
                        "amount": 10,
                        "affiliate_id": 42,
                        "type": "sale_commission",
                        "status": "approved",
                        "level": 1,
                        "order_id": 1232122
                      }
                    ]
                  }
                }
              },
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "rewards": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "amount": {
                            "type": "integer",
                            "required": true,
                            "description": "Reward amount to give to the affiliate"
                          },
                          "type": {
                            "type": "string",
                            "enum": [
                              "signup_bonus",
                              "sale_commission",
                              "recruitment_bonus",
                              "target_bonus"
                            ],
                            "required": true
                          },
                          "metadata": {
                            "type": "string",
                            "description": "Any piece of metadata you wish to attach with this entry"
                          },
                          "order_id": {
                            "type": "string",
                            "description": "Required for reward type of sale_commission. The order id for whom this upline level commission is given"
                          },
                          "level": {
                            "type": "string",
                            "description": "Optional. Only valid for reward type of sale_commission. The upline level number"
                          },
                          "status": {
                            "type": "string",
                            "enum": [
                              "pending",
                              "approved",
                              "rejected"
                            ]
                          }
                        }
                      }
                    }
                  },
                  "example": {
                    "rewards": [
                      {
                        "amount": 10,
                        "affiliate_id": 42,
                        "type": "sale_commission",
                        "status": "approved",
                        "level": 1,
                        "order_id": 1232122
                      }
                    ]
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "success": {
                        "type": "boolean"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/rewards/{id}": {
        "delete": {
          "tags": [
            "rewards"
          ],
          "summary": "Delete a given reward",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "path",
              "name": "id",
              "description": "ID of the reward to delete"
            }
          ]
        },
        "patch": {
          "tags": [
            "rewards"
          ],
          "summary": "Updates a given reward",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "path",
              "name": "id",
              "description": "ID of the reward to update"
            }
          ],
          "requestBody": {
            "content": {
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "amount": {
                      "type": "integer"
                    },
                    "status": {
                      "type": "string",
                      "enum": [
                        "approved",
                        "rejected"
                      ]
                    }
                  }
                }
              },
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "amount": {
                      "type": "integer"
                    },
                    "status": {
                      "type": "string",
                      "enum": [
                        "approved",
                        "rejected"
                      ]
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/payments": {
        "get": {
          "tags": [
            "payments"
          ],
          "summary": "List payment history",
          "security": [
            {
              "admin": []
            }
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "query",
              "name": "id",
              "description": "Retrieve only the payments specified by a comma-separated list of payment IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "affiliate_id",
              "description": "Retrieve only the payments done to affiliates specified by a comma-separated list of affiliate IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "created_at_max",
              "description": "Show payments history created at or before date",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "created_at_min",
              "description": "Show payments history created at or after date",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Show payments history after specified payment ID",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of items in result",
              "schema": {
                "type": "number"
              }
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Fetch results after this offset",
              "schema": {
                "type": "number"
              }
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return. See Affiliate Schema to get list of available fields",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "affiliate_id",
                    "amount",
                    "currency",
                    "payment_method",
                    "payment_details",
                    "affiliate_message",
                    "admin_note",
                    "transactions",
                    "created_at"
                  ]
                }
              },
              "explode": false,
              "required": true
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "payments": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "integer",
                              "description": "Payout ID"
                            },
                            "amount": {
                              "type": "number",
                              "description": "The amount paid in the payout"
                            },
                            "currency": {
                              "type": "number",
                              "description": "The currency for the amount"
                            },
                            "affiliate_id": {
                              "type": "number",
                              "description": "ID of the affiliate who was paid"
                            },
                            "payment_method": {
                              "type": "string",
                              "description": "Payment method of the affiliate for this payout"
                            },
                            "payment_details": {
                              "type": "object",
                              "description": "The payment details"
                            },
                            "admin_note": {
                              "type": "string",
                              "description": "Private note written by admin"
                            },
                            "affiliate_message": {
                              "type": "string",
                              "description": "The message for the affiliate"
                            },
                            "created_at": {
                              "type": "string",
                              "description": "The date when payout was created"
                            }
                          },
                          "example": {
                            "id": 1,
                            "affiliate_id": 1,
                            "amount": 100,
                            "payment_method": "paypal",
                            "payment_details": {
                              "paypal_email": "abc@paypal.com"
                            },
                            "admin_note": "Paid for the orders",
                            "affiliate_message": "Here is your payment",
                            "created_at": "Wed Feb 18 2026 07:38:43 GMT+0100 (Central European Standard Time)"
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "payments"
          ],
          "summary": "Mark affiliates as paid in bulk",
          "security": [
            {
              "admin": []
            }
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "payments": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "affiliate_id": {
                            "type": "string",
                            "required": true
                          },
                          "amount": {
                            "type": "number",
                            "required": true
                          },
                          "created_at": {
                            "type": "string",
                            "description": "The date the payout was sent. If nothing is set, **now** is used the the date"
                          },
                          "affiliate_message": {
                            "type": "string",
                            "description": "A message to send to this affiliate"
                          },
                          "admin_note": {
                            "type": "string",
                            "description": "A note for admin"
                          },
                          "payment_method": {
                            "type": "string",
                            "description": "The payment method via which the payment was made. Leave empty to use affiliate's set payment method for this payout"
                          },
                          "payment_details": {
                            "type": "object",
                            "description": "Payment details for the payment method. Leave empty to use affiliate's set payment method for this payout"
                          },
                          "tx_ids": {
                            "type": "array",
                            "description": "Optional array of transaction IDs to mark as paid",
                            "items": {
                              "type": "number"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "payments": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "integer",
                              "description": "Payout ID"
                            },
                            "amount": {
                              "type": "number",
                              "description": "The amount paid in the payout"
                            },
                            "currency": {
                              "type": "number",
                              "description": "The currency for the amount"
                            },
                            "affiliate_id": {
                              "type": "number",
                              "description": "ID of the affiliate who was paid"
                            },
                            "payment_method": {
                              "type": "string",
                              "description": "Payment method of the affiliate for this payout"
                            },
                            "payment_details": {
                              "type": "object",
                              "description": "The payment details"
                            },
                            "admin_note": {
                              "type": "string",
                              "description": "Private note written by admin"
                            },
                            "affiliate_message": {
                              "type": "string",
                              "description": "The message for the affiliate"
                            },
                            "created_at": {
                              "type": "string",
                              "description": "The date when payout was created"
                            }
                          },
                          "example": {
                            "id": 1,
                            "affiliate_id": 1,
                            "amount": 100,
                            "payment_method": "paypal",
                            "payment_details": {
                              "paypal_email": "abc@paypal.com"
                            },
                            "admin_note": "Paid for the orders",
                            "affiliate_message": "Here is your payment",
                            "created_at": "Wed Feb 18 2026 07:38:43 GMT+0100 (Central European Standard Time)"
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/payments/{id}": {
        "delete": {
          "tags": [
            "payments"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "summary": "Deletes a payment entry",
          "parameters": [
            {
              "in": "path",
              "name": "id",
              "description": "Payment ID"
            }
          ]
        },
        "patch": {
          "tags": [
            "payments"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "summary": "Updates a payment entry",
          "parameters": [
            {
              "in": "path",
              "name": "id",
              "description": "Payment ID"
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "admin_note": {
                      "type": "string",
                      "description": "Private note written by admin"
                    },
                    "affiliate_message": {
                      "type": "string",
                      "description": "The message for the affiliate"
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "success": {
                        "type": "integer",
                        "description": "ID of the payment"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/payments/requests": {
        "get": {
          "tags": [
            "payments"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "summary": "Lists payment requests made by the affiliate from their dashboard",
          "parameters": [
            {
              "in": "query",
              "name": "affiliate_id",
              "description": "Retrieve only the payment history for affiliates specified by a comma-separated list of affiliate IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Show payments requests after specified request ID",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "status",
              "description": "Filters results by the status",
              "schema": {
                "type": "string",
                "enum": [
                  "in_progress",
                  "paid",
                  "rejected"
                ]
              }
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of items in result",
              "schema": {
                "type": "number"
              }
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "PaymentRequests": {
                        "type": "array",
                        "items": {
                          "$ref": "#/components/schemas/PaymentRequests"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "payments"
          ],
          "summary": "Create a new payment request",
          "security": [
            {
              "admin": []
            }
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "affiliate_id": {
                      "type": "number",
                      "description": "ID of the affiliate making the request",
                      "required": true
                    },
                    "tx_ids": {
                      "type": "array",
                      "description": "Array of transaction IDs to include in the payment request",
                      "items": {
                        "type": "number"
                      },
                      "required": true
                    },
                    "note": {
                      "type": "string",
                      "description": "Optional note for the payment request"
                    },
                    "invoice_url": {
                      "type": "string",
                      "description": "Optional invoice URL for the payment request"
                    }
                  }
                },
                "example": {
                  "affiliate_id": 1,
                  "tx_ids": [
                    123,
                    456,
                    789
                  ],
                  "note": "Payment request for Q4 commissions",
                  "invoice_url": "https://example.com/invoice.pdf"
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "id": {
                        "type": "number",
                        "description": "ID of the created payment request"
                      },
                      "amount": {
                        "type": "number",
                        "description": "Total amount of the payment request"
                      },
                      "note": {
                        "type": "string",
                        "description": "Note for the payment request"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/payments/requests/{id}": {
        "patch": {
          "tags": [
            "payments"
          ],
          "summary": "Updates the status of the payment request",
          "security": [
            {
              "admin": []
            }
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "path",
              "name": "id",
              "description": "Request ID of the payment request to update",
              "schema": {
                "type": "string"
              }
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "status": {
                      "type": "string",
                      "enum": [
                        "in_progress",
                        "paid",
                        "rejected"
                      ]
                    }
                  }
                }
              }
            },
            "example": {
              "status": "paid"
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "status": {
                        "type": "string",
                        "enum": [
                          "in_progress",
                          "paid",
                          "rejected"
                        ]
                      },
                      "success": {
                        "type": "boolean"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/payments/pending": {
        "get": {
          "tags": [
            "payments"
          ],
          "summary": "Lists amount due to the affiliates",
          "security": [
            {
              "admin": []
            }
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "query",
              "name": "affiliate_id",
              "description": "Retrieve only the pending amounts for affiliates specified by a comma-separated list of affiliate IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "upto",
              "description": "Retrieves amounts due up until specified date",
              "schema": {
                "type": "string"
              }
            }
          ],
          "responses": {
            "200": {
              "description": null,
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "pending": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "affiliate_id": {
                              "type": "string"
                            },
                            "amount": {
                              "type": "string"
                            }
                          }
                        }
                      },
                      "upto": {
                        "type": "string",
                        "description": "Date up until the amount is owed"
                      }
                    }
                  }
                }
              },
              "example": {
                "pending": [
                  {
                    "affiliate_id": 1,
                    "amount": 100,
                    "total_earned": 300,
                    "total_paid": 200
                  },
                  {
                    "affiliate_id": 2,
                    "amount": 200,
                    "total_earned": 800,
                    "total_paid": 600
                  }
                ]
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/mlm/tree": {
        "get": {
          "tags": [
            "multi-level"
          ],
          "summary": "Lists the multi level network",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "tree": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "string",
                              "description": "Affiliate ID"
                            },
                            "parent": {
                              "type": "string",
                              "description": "ID of the parent of the affiliate"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/mlm/parents/{affiliate_id}": {
        "get": {
          "tags": [
            "multi-level"
          ],
          "summary": "Lists the parent affiliate IDs for this affiliate",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "path",
              "name": "affiliate_id",
              "description": "ID of the affiliate to retrieve parents for"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "parents": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "string",
                              "description": "Affiliate ID"
                            },
                            "parent": {
                              "type": "string",
                              "description": "ID of the parent of the affiliate"
                            },
                            "level": {
                              "type": "string",
                              "description": "Level of this parent with respect to the queried affiliate id"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/mlm/move/{id}": {
        "post": {
          "tags": [
            "multi-level"
          ],
          "summary": "Moves an affiliate from one place in the tree to another.",
          "description": "This also moves their downlines with them",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "path",
              "name": "id",
              "description": "ID of the affiliate to move",
              "required": true
            }
          ],
          "requestBody": {
            "content": {
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "new_parent": {
                      "type": "string",
                      "description": "Omit this if you want to disconnect parents from this affiliate"
                    }
                  }
                }
              },
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "new_parent": {
                      "type": "string",
                      "description": "Omit this if you want to disconnect parents from this affiliate"
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "id": {
                        "type": "string"
                      },
                      "new_parent": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/affiliates": {
        "get": {
          "tags": [
            "affiliate"
          ],
          "description": "Retrieves list of affiliates.",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "query",
              "name": "id",
              "description": "Retrieve only affiliates specified by a comma-separated list of affiliate IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "ref_code",
              "description": "Retrieve only affiliates specified by a comma-separated list of their referral codes",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "email",
              "description": "Retrieve only affiliates specified by a comma-separated list of their email addresses",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "coupon",
              "description": "Retrieve only affiliates specified by a comma-separated list of their coupon codes",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "status",
              "description": "Retrieve only affiliates specified by a comma-separated list of their approval status",
              "schema": {
                "type": "string",
                "enum": [
                  "pending",
                  "approved",
                  "invited",
                  "rejected"
                ]
              }
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Show affiliates after the specified affiliate ID",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "created_at_max",
              "schema": {
                "type": "string"
              },
              "description": "Show affiliates created at or before date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "created_at_min",
              "schema": {
                "type": "string"
              },
              "description": "Show affiliates created at or after date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "updated_at_max",
              "schema": {
                "type": "string"
              },
              "description": "Show affiliates updated at or before date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "updated_at_min",
              "schema": {
                "type": "string"
              },
              "description": "Show affiliates updated at or after date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return. See Affiliate Schema to get list of available fields",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "avatar",
                    "honorific",
                    "date_of_birth",
                    "gender",
                    "name",
                    "first_name",
                    "last_name",
                    "email",
                    "ref_code",
                    "company_name",
                    "ref_codes",
                    "coupon",
                    "coupons",
                    "phone",
                    "website",
                    "facebook",
                    "twitter",
                    "instagram",
                    "address_1",
                    "address_2",
                    "city",
                    "state",
                    "zip",
                    "country",
                    "phone",
                    "admin_note",
                    "extra_1",
                    "extra_2",
                    "extra_3",
                    "group_id",
                    "registration_ip",
                    "personal_message",
                    "payment_method",
                    "payment_details",
                    "commission",
                    "status",
                    "last_login",
                    "total_referral_earnings",
                    "total_network_earnings",
                    "total_amount_paid",
                    "total_amount_pending",
                    "total_other_earnings",
                    "number_of_orders",
                    "tax_identification_number",
                    "login_token",
                    "signup_page",
                    "comments",
                    "tags",
                    "approved_at",
                    "blocked_at",
                    "created_at",
                    "updated_at"
                  ]
                }
              },
              "explode": false,
              "required": true
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset of results",
              "type": "integer"
            }
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "affiliates": {
                        "type": "array",
                        "items": {
                          "$ref": "#/components/schemas/Affiliate"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "affiliate"
          ],
          "description": "Create a new affiliate",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "email": {
                      "type": "string",
                      "required": true
                    },
                    "password": {
                      "type": "string"
                    },
                    "password_expired": {
                      "type": "boolean",
                      "description": "Prompts the affiliates to change password upon login"
                    },
                    "name": {
                      "type": "string"
                    },
                    "first_name": {
                      "type": "string"
                    },
                    "last_name": {
                      "type": "string"
                    },
                    "date_of_birth": {
                      "type": "string"
                    },
                    "honorific": {
                      "type": "string"
                    },
                    "gender": {
                      "type": "string"
                    },
                    "facebook": {
                      "type": "string"
                    },
                    "twitter": {
                      "type": "string"
                    },
                    "instagram": {
                      "type": "string"
                    },
                    "website": {
                      "type": "string"
                    },
                    "city": {
                      "type": "string"
                    },
                    "state": {
                      "type": "string"
                    },
                    "country": {
                      "type": "string"
                    },
                    "zip": {
                      "type": "string"
                    },
                    "phone": {
                      "type": "string"
                    },
                    "address_1": {
                      "type": "string"
                    },
                    "address_2": {
                      "type": "string"
                    },
                    "company_name": {
                      "type": "string"
                    },
                    "tax_identification_number": {
                      "type": "string"
                    },
                    "extra_1": {
                      "type": "string"
                    },
                    "extra_2": {
                      "type": "string"
                    },
                    "extra_3": {
                      "type": "string"
                    },
                    "ref_code": {
                      "type": "string"
                    },
                    "admin_note": {
                      "type": "string"
                    },
                    "personal_message": {
                      "type": "string"
                    },
                    "comments": {
                      "type": "string"
                    },
                    "updated_at": {
                      "type": "string"
                    },
                    "created_at": {
                      "type": "string"
                    },
                    "coupon": {
                      "type": "object",
                      "properties": {
                        "discount_type": {
                          "type": "string",
                          "enum": [
                            "fixed_amount",
                            "percentage"
                          ]
                        },
                        "discount_value": {
                          "type": "integer"
                        }
                      }
                    },
                    "status": {
                      "type": "string",
                      "enum": [
                        "approved",
                        "pending",
                        "rejected"
                      ]
                    },
                    "parent_email": {
                      "type": "string",
                      "description": "Email address of the parent affiliate. Useful for building MLM hierarchies"
                    }
                  }
                },
                "example": {
                  "email": "jdoe@example.com",
                  "password": "secretpassword",
                  "password_expired": true,
                  "honorific": "Mr",
                  "name": "John Doe",
                  "gender": "Male",
                  "date_of_birth": "28 June 1988",
                  "phone": "+1 555 (1220) 1212",
                  "company_name": "Acme Inc!",
                  "commission": {
                    "type": "percentage",
                    "amount": "10"
                  },
                  "ref_code": "refferralcode",
                  "coupon": {
                    "code": "ABC",
                    "discount_type": "percentage",
                    "discount_value": 10
                  },
                  "address_1": "Address line 1",
                  "address_2": "Address line 2",
                  "city": "City",
                  "state": "State or Province",
                  "zip": "10011",
                  "country": "US",
                  "facebook": "fb",
                  "instagram": "instagram handle",
                  "twitter": "twitter handle",
                  "snapchat": "snapchat username",
                  "pinterest": "pinterest username",
                  "tax_identification_number": "111-2222-333",
                  "paypal_email": "jdoe@paypal.com",
                  "status": "approved",
                  "parent_email": "abc@xyz.com",
                  "updated_at": "2019-07-26T06:40:07.000Z",
                  "created_at": "2019-07-26T06:40:07.000Z",
                  "profile_photo": "https://example.com/profile.jpg"
                }
              },
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "email": {
                      "type": "string",
                      "required": true
                    },
                    "password": {
                      "type": "string"
                    },
                    "password_expired": {
                      "type": "boolean",
                      "description": "Prompts the affiliates to change password upon login"
                    },
                    "name": {
                      "type": "string"
                    },
                    "first_name": {
                      "type": "string"
                    },
                    "last_name": {
                      "type": "string"
                    },
                    "date_of_birth": {
                      "type": "string"
                    },
                    "honorific": {
                      "type": "string"
                    },
                    "gender": {
                      "type": "string"
                    },
                    "facebook": {
                      "type": "string"
                    },
                    "twitter": {
                      "type": "string"
                    },
                    "instagram": {
                      "type": "string"
                    },
                    "website": {
                      "type": "string"
                    },
                    "city": {
                      "type": "string"
                    },
                    "state": {
                      "type": "string"
                    },
                    "country": {
                      "type": "string"
                    },
                    "zip": {
                      "type": "string"
                    },
                    "phone": {
                      "type": "string"
                    },
                    "address_1": {
                      "type": "string"
                    },
                    "address_2": {
                      "type": "string"
                    },
                    "company_name": {
                      "type": "string"
                    },
                    "tax_identification_number": {
                      "type": "string"
                    },
                    "extra_1": {
                      "type": "string"
                    },
                    "extra_2": {
                      "type": "string"
                    },
                    "extra_3": {
                      "type": "string"
                    },
                    "ref_code": {
                      "type": "string"
                    },
                    "admin_note": {
                      "type": "string"
                    },
                    "personal_message": {
                      "type": "string"
                    },
                    "comments": {
                      "type": "string"
                    },
                    "updated_at": {
                      "type": "string"
                    },
                    "created_at": {
                      "type": "string"
                    },
                    "coupon": {
                      "type": "object",
                      "properties": {
                        "discount_type": {
                          "type": "string",
                          "enum": [
                            "fixed_amount",
                            "percentage"
                          ]
                        },
                        "discount_value": {
                          "type": "integer"
                        }
                      }
                    },
                    "status": {
                      "type": "string",
                      "enum": [
                        "approved",
                        "pending",
                        "rejected"
                      ]
                    },
                    "parent_email": {
                      "type": "string",
                      "description": "Email address of the parent affiliate. Useful for building MLM hierarchies"
                    }
                  }
                },
                "example": {
                  "email": "jdoe@example.com",
                  "password": "secretpassword",
                  "password_expired": true,
                  "honorific": "Mr",
                  "name": "John Doe",
                  "gender": "Male",
                  "date_of_birth": "28 June 1988",
                  "phone": "+1 555 (1220) 1212",
                  "company_name": "Acme Inc!",
                  "commission": {
                    "type": "percentage",
                    "amount": "10"
                  },
                  "ref_code": "refferralcode",
                  "coupon": {
                    "code": "ABC",
                    "discount_type": "percentage",
                    "discount_value": 10
                  },
                  "address_1": "Address line 1",
                  "address_2": "Address line 2",
                  "city": "City",
                  "state": "State or Province",
                  "zip": "10011",
                  "country": "US",
                  "facebook": "fb",
                  "instagram": "instagram handle",
                  "twitter": "twitter handle",
                  "snapchat": "snapchat username",
                  "pinterest": "pinterest username",
                  "tax_identification_number": "111-2222-333",
                  "paypal_email": "jdoe@paypal.com",
                  "status": "approved",
                  "parent_email": "abc@xyz.com",
                  "updated_at": "2019-07-26T06:40:07.000Z",
                  "created_at": "2019-07-26T06:40:07.000Z",
                  "profile_photo": "https://example.com/profile.jpg"
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "affiliate_id": {
                        "type": "number"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/affiliates/search": {
        "get": {
          "tags": [
            "affiliate"
          ],
          "description": "Searches for the affiliates",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "query",
              "name": "in",
              "description": "The columns on which to search for the specified keyword",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "name",
                    "ref_code",
                    "email",
                    "coupon"
                  ]
                }
              },
              "required": true,
              "explode": false
            },
            {
              "in": "query",
              "name": "keyword",
              "description": "The keyword to search for",
              "schema": {
                "type": "string"
              },
              "required": true
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return. See Affiliate Schema to get list of available fields",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "name",
                    "first_name",
                    "last_name",
                    "email",
                    "ref_code",
                    "ref_codes",
                    "coupon",
                    "coupons",
                    "phone",
                    "facebook",
                    "twitter",
                    "instagram",
                    "address_1",
                    "address_2",
                    "city",
                    "state",
                    "zip",
                    "country",
                    "phone",
                    "extra_1",
                    "extra_2",
                    "extra_3",
                    "group_id",
                    "registration_ip",
                    "personal_message",
                    "admin_note",
                    "payment_method",
                    "payment_details",
                    "commission",
                    "status",
                    "last_login",
                    "total_referral_earnings",
                    "total_network_earnings",
                    "total_amount_paid",
                    "total_amount_pending",
                    "total_other_earnings",
                    "number_of_orders"
                  ]
                }
              },
              "explode": false,
              "required": true
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "sort_direction",
              "description": "The direction to sort the results",
              "schema": {
                "type": "string",
                "enum": [
                  "asc",
                  "desc"
                ]
              }
            },
            {
              "in": "query",
              "name": "sort_column",
              "description": "The column to sort the results by",
              "schema": {
                "type": "string",
                "enum": [
                  "name",
                  "ref_code",
                  "id"
                ]
              }
            },
            {
              "in": "query",
              "name": "operator",
              "description": "The operator to use for matching the keyword",
              "schema": {
                "type": "string",
                "enum": [
                  "starts_with",
                  "ends_with",
                  "contains"
                ],
                "default": "starts_with"
              }
            }
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "affiliates": {
                        "type": "array",
                        "items": {
                          "$ref": "#/components/schemas/Affiliate"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/affiliates/{id}": {
        "parameters": [
          {
            "in": "path",
            "name": "id",
            "description": "ID of the affiliate",
            "schema": {
              "type": "string"
            },
            "required": true
          }
        ],
        "delete": {
          "tags": [
            "affiliate"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "produces": [
            "application/json"
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "affiliate_id": {
                        "type": "integer",
                        "description": "Affiliate ID of the deleted affiliate"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "patch": {
          "tags": [
            "affiliate"
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "name": {
                      "type": "string"
                    },
                    "first_name": {
                      "type": "string"
                    },
                    "last_name": {
                      "type": "string"
                    },
                    "company_name": {
                      "type": "string"
                    },
                    "ref_code": {
                      "type": "string"
                    },
                    "address_1": {
                      "type": "string"
                    },
                    "address_2": {
                      "type": "string"
                    },
                    "city": {
                      "type": "string"
                    },
                    "state": {
                      "type": "string"
                    },
                    "country": {
                      "type": "string"
                    },
                    "phone": {
                      "type": "string"
                    },
                    "zip": {
                      "type": "string"
                    },
                    "facebook": {
                      "type": "string"
                    },
                    "twitter": {
                      "type": "string"
                    },
                    "instagram": {
                      "type": "string"
                    },
                    "website": {
                      "type": "string"
                    },
                    "tax_identification_number": {
                      "type": "string"
                    },
                    "personal_message": {
                      "type": "string"
                    },
                    "admin_note": {
                      "type": "string"
                    },
                    "extra_1": {
                      "type": "string"
                    },
                    "extra_2": {
                      "type": "string"
                    },
                    "extra_3": {
                      "type": "string"
                    },
                    "profile_photo": {
                      "type": "string"
                    },
                    "commission": {
                      "type": "object",
                      "properties": {
                        "type": {
                          "type": "string",
                          "enum": [
                            "percentage",
                            "fixed_amount"
                          ]
                        },
                        "amount": {
                          "type": "integer",
                          "minimum": 0,
                          "format": "double"
                        },
                        "on": {
                          "type": "string",
                          "enum": [
                            "product",
                            "order"
                          ]
                        }
                      }
                    },
                    "coupon": {
                      "type": "object",
                      "properties": {
                        "discount_type": {
                          "type": "string",
                          "enum": [
                            "percentage",
                            "fixed_amount"
                          ]
                        },
                        "discount_value": {
                          "type": "integer",
                          "minimum": 0,
                          "format": "double"
                        },
                        "code": {
                          "type": "string"
                        }
                      }
                    },
                    "status": {
                      "type": "string",
                      "enum": [
                        "pending",
                        "approved",
                        "rejected"
                      ]
                    }
                  }
                },
                "example": {
                  "password": "new-password",
                  "honorific": "Mr",
                  "name": "John Doe",
                  "gender": "Male",
                  "date_of_birth": "28 June 1988",
                  "phone": "+1 555 (1220) 1212",
                  "company_name": "Acme Inc!",
                  "commission": {
                    "type": "percentage",
                    "amount": "10"
                  },
                  "ref_code": "refferralcode",
                  "coupon": {
                    "code": "ABC",
                    "discount_type": "percentage",
                    "discount_value": 10
                  },
                  "address_1": "Address line 1",
                  "address_2": "Address line 2",
                  "city": "City",
                  "state": "State or Province",
                  "zip": "10011",
                  "country": "US",
                  "facebook": "fb",
                  "instagram": "instagram handle",
                  "twitter": "twitter handle",
                  "snapchat": "snapchat username",
                  "pinterest": "pinterest username",
                  "tax_identification_number": "111-2222-333",
                  "paypal_email": "jdoe@paypal.com",
                  "status": "approved",
                  "parent_email": "abc@xyz.com",
                  "profile_photo": "https://example.com/profile.jpg"
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "schema": {
                      "type": "object",
                      "properties": {
                        "name": {
                          "type": "string"
                        },
                        "first_name": {
                          "type": "string"
                        },
                        "last_name": {
                          "type": "string"
                        },
                        "company_name": {
                          "type": "string"
                        },
                        "ref_code": {
                          "type": "string"
                        },
                        "address_1": {
                          "type": "string"
                        },
                        "address_2": {
                          "type": "string"
                        },
                        "city": {
                          "type": "string"
                        },
                        "state": {
                          "type": "string"
                        },
                        "country": {
                          "type": "string"
                        },
                        "phone": {
                          "type": "string"
                        },
                        "zip": {
                          "type": "string"
                        },
                        "facebook": {
                          "type": "string"
                        },
                        "twitter": {
                          "type": "string"
                        },
                        "instagram": {
                          "type": "string"
                        },
                        "website": {
                          "type": "string"
                        },
                        "tax_identification_number": {
                          "type": "string"
                        },
                        "personal_message": {
                          "type": "string"
                        },
                        "admin_note": {
                          "type": "string"
                        },
                        "extra_1": {
                          "type": "string"
                        },
                        "extra_2": {
                          "type": "string"
                        },
                        "extra_3": {
                          "type": "string"
                        },
                        "profile_photo": {
                          "type": "string"
                        },
                        "commission": {
                          "type": "object",
                          "properties": {
                            "type": {
                              "type": "string",
                              "enum": [
                                "percentage",
                                "fixed_amount"
                              ]
                            },
                            "amount": {
                              "type": "integer",
                              "minimum": 0,
                              "format": "double"
                            },
                            "on": {
                              "type": "string",
                              "enum": [
                                "product",
                                "order"
                              ]
                            }
                          }
                        },
                        "coupon": {
                          "type": "object",
                          "properties": {
                            "discount_type": {
                              "type": "string",
                              "enum": [
                                "percentage",
                                "fixed_amount"
                              ]
                            },
                            "discount_value": {
                              "type": "integer",
                              "minimum": 0,
                              "format": "double"
                            },
                            "code": {
                              "type": "string"
                            }
                          }
                        },
                        "status": {
                          "type": "string",
                          "enum": [
                            "pending",
                            "approved",
                            "rejected"
                          ]
                        }
                      }
                    },
                    "example": {
                      "password": "new-password",
                      "honorific": "Mr",
                      "name": "John Doe",
                      "gender": "Male",
                      "date_of_birth": "28 June 1988",
                      "phone": "+1 555 (1220) 1212",
                      "company_name": "Acme Inc!",
                      "commission": {
                        "type": "percentage",
                        "amount": "10"
                      },
                      "ref_code": "refferralcode",
                      "coupon": {
                        "code": "ABC",
                        "discount_type": "percentage",
                        "discount_value": 10
                      },
                      "address_1": "Address line 1",
                      "address_2": "Address line 2",
                      "city": "City",
                      "state": "State or Province",
                      "zip": "10011",
                      "country": "US",
                      "facebook": "fb",
                      "instagram": "instagram handle",
                      "twitter": "twitter handle",
                      "snapchat": "snapchat username",
                      "pinterest": "pinterest username",
                      "tax_identification_number": "111-2222-333",
                      "paypal_email": "jdoe@paypal.com",
                      "status": "approved",
                      "parent_email": "abc@xyz.com",
                      "profile_photo": "https://example.com/profile.jpg"
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/affiliates/{id}/commissions": {
        "parameters": [
          {
            "in": "path",
            "name": "id",
            "description": "ID of the affiliate",
            "schema": {
              "type": "string"
            },
            "required": true
          }
        ],
        "get": {
          "tags": [
            "affiliate"
          ],
          "description": "Retrieves list coupons given to this affiliate.",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "commissions": {
                        "type": "object",
                        "properties": {
                          "standard": {
                            "type": "object"
                          },
                          "special": {
                            "type": "object"
                          },
                          "royalties": {
                            "type": "object"
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/affiliates/{id}/coupons": {
        "parameters": [
          {
            "in": "path",
            "name": "id",
            "description": "ID of the affiliate",
            "schema": {
              "type": "string"
            },
            "required": true
          }
        ],
        "get": {
          "tags": [
            "affiliate"
          ],
          "description": "Retrieves list coupons given to this affiliate.",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "coupons": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "code": {
                              "type": "string"
                            },
                            "discount_type": {
                              "type": "string",
                              "enum": [
                                "percentage",
                                "fixed_amount",
                                "free_shipping"
                              ]
                            },
                            "discount_value": {
                              "type": "number"
                            }
                          }
                        }
                      }
                    }
                  },
                  "example": {
                    "coupons": [
                      {
                        "code": "JOHN10OFF",
                        "discount_type": "percentage",
                        "discount_value": 10
                      },
                      {
                        "code": "JOHN2NDCOUPON",
                        "discount_type": "percentage",
                        "discount_value": 20
                      }
                    ]
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "affiliate"
          ],
          "description": "Adds a coupon code to the affiliate's account",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "oneOf": [
                    {
                      "type": "object",
                      "properties": {
                        "coupon": {
                          "type": "object",
                          "description": "Adds the coupon to the affiliate's account",
                          "properties": {
                            "code": {
                              "type": "string"
                            },
                            "discount_type": {
                              "type": "string",
                              "enum": [
                                "percentage",
                                "fixed_amount",
                                "free_shipping"
                              ]
                            },
                            "discount_value": {
                              "type": "number"
                            }
                          }
                        }
                      }
                    },
                    {
                      "type": "object",
                      "description": "Creates the discount code as per the automatic coupons setting of the store",
                      "properties": {
                        "coupon": {
                          "type": "string"
                        }
                      }
                    }
                  ]
                },
                "examples": [
                  {
                    "value": "",
                    "summary": "Choose an example"
                  },
                  {
                    "value": {
                      "coupon": {
                        "code": "JOHN10OFF",
                        "discount_type": "percentage",
                        "discount_value": 10
                      }
                    },
                    "summary": "Only assigns the coupon code to the affiliate"
                  },
                  {
                    "value": {
                      "coupon": "JOHN10OFF"
                    },
                    "summary": "Assigns as well as creates the coupon code in the store"
                  }
                ]
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "coupons": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "code": {
                              "type": "string"
                            },
                            "discount_type": {
                              "type": "string",
                              "enum": [
                                "percentage",
                                "fixed_amount",
                                "free_shipping"
                              ]
                            },
                            "discount_value": {
                              "type": "number"
                            }
                          }
                        }
                      }
                    }
                  },
                  "example": {
                    "coupons": [
                      {
                        "code": "JOHN10OFF",
                        "discount_type": "percentage",
                        "discount_value": 10
                      },
                      {
                        "code": "JOHN2NDCOUPON",
                        "discount_type": "percentage",
                        "discount_value": 20
                      }
                    ]
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/affiliates/{id}/coupons/{coupon_code}": {
        "parameters": [
          {
            "in": "path",
            "name": "id",
            "description": "ID of the affiliate",
            "schema": {
              "type": "string"
            },
            "required": true
          },
          {
            "in": "path",
            "name": "coupon_code",
            "description": "Coupon code that you want to delete from the affiliates account",
            "schema": {
              "type": "string"
            },
            "required": true
          }
        ],
        "delete": {
          "tags": [
            "affiliate"
          ],
          "description": "Deletes coupon code from affiliate's account",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "coupons": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "code": {
                              "type": "string"
                            },
                            "discount_type": {
                              "type": "string",
                              "enum": [
                                "percentage",
                                "fixed_amount",
                                "free_shipping"
                              ]
                            },
                            "discount_value": {
                              "type": "number"
                            }
                          }
                        }
                      }
                    }
                  },
                  "example": {
                    "coupons": [
                      {
                        "code": "JOHN10OFF",
                        "discount_type": "percentage",
                        "discount_value": 10
                      },
                      {
                        "code": "JOHN2NDCOUPON",
                        "discount_type": "percentage",
                        "discount_value": 20
                      }
                    ]
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/affiliates/{id}/referral_codes": {
        "parameters": [
          {
            "in": "path",
            "name": "id",
            "description": "ID of the affiliate",
            "schema": {
              "type": "string"
            },
            "required": true
          }
        ],
        "get": {
          "tags": [
            "affiliate"
          ],
          "description": "Retrieves list referral codes given to this affiliate.",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "coupons": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "code": {
                              "type": "string"
                            },
                            "discount_type": {
                              "type": "string",
                              "enum": [
                                "percentage",
                                "fixed_amount",
                                "free_shipping"
                              ]
                            },
                            "discount_value": {
                              "type": "number"
                            }
                          }
                        }
                      }
                    }
                  },
                  "example": {
                    "coupons": [
                      {
                        "code": "JOHN10OFF",
                        "discount_type": "percentage",
                        "discount_value": 10
                      },
                      {
                        "code": "JOHN2NDCOUPON",
                        "discount_type": "percentage",
                        "discount_value": 20
                      }
                    ]
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "put": {
          "tags": [
            "affiliate"
          ],
          "description": "Replaces the affiliate's referral codes with the new list of referral codes",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "referral_codes": {
                      "type": "array",
                      "items": {
                        "type": "string"
                      }
                    }
                  },
                  "example": {
                    "referral_codes": [
                      "ABC",
                      "123"
                    ]
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "referral_codes": {
                        "type": "array",
                        "items": {
                          "type": "string"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "affiliate"
          ],
          "description": "Adds the list of codes to the affiliate's account",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "referral_codes": {
                      "type": "array",
                      "items": {
                        "type": "string"
                      }
                    }
                  },
                  "example": {
                    "referral_codes": [
                      "ABC",
                      "123"
                    ]
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "referral_codes": {
                        "type": "array",
                        "items": {
                          "type": "string"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/affiliates/{id}/referral_codes/{referral_code}": {
        "parameters": [
          {
            "in": "path",
            "name": "id",
            "description": "ID of the affiliate",
            "schema": {
              "type": "string"
            },
            "required": true
          },
          {
            "in": "path",
            "name": "referral_code",
            "description": "Referral code that you want to delete from the affiliates account",
            "schema": {
              "type": "string"
            },
            "required": true
          }
        ],
        "delete": {
          "tags": [
            "affiliate"
          ],
          "description": "Deletes referral code from affiliate's account",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "coupons": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "code": {
                              "type": "string"
                            },
                            "discount_type": {
                              "type": "string",
                              "enum": [
                                "percentage",
                                "fixed_amount",
                                "free_shipping"
                              ]
                            },
                            "discount_value": {
                              "type": "number"
                            }
                          }
                        }
                      }
                    }
                  },
                  "example": {
                    "coupons": [
                      {
                        "code": "JOHN10OFF",
                        "discount_type": "percentage",
                        "discount_value": 10
                      },
                      {
                        "code": "JOHN2NDCOUPON",
                        "discount_type": "percentage",
                        "discount_value": 20
                      }
                    ]
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/affiliates/{id}/tags": {
        "parameters": [
          {
            "in": "path",
            "name": "id",
            "description": "ID of the affiliate",
            "schema": {
              "type": "string"
            },
            "required": true
          }
        ],
        "get": {
          "tags": [
            "affiliate"
          ],
          "description": "Retrieves list tags attached to this affiliate.",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "tags": {
                        "type": "array",
                        "items": {
                          "type": "string"
                        }
                      }
                    }
                  },
                  "example": {
                    "tags": [
                      "tag-1",
                      "tag-2"
                    ]
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "put": {
          "tags": [
            "affiliate"
          ],
          "description": "Replaces the affiliate's referral codes with the new list of referral codes",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "tags": {
                      "type": "array",
                      "items": {
                        "type": "string"
                      }
                    }
                  },
                  "example": {
                    "tags": [
                      "tag-1",
                      "tag-2"
                    ]
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "tags": {
                        "type": "array",
                        "items": {
                          "type": "string"
                        }
                      }
                    }
                  },
                  "example": {
                    "tags": [
                      "tag-1",
                      "tag-2"
                    ]
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/connections": {
        "get": {
          "tags": [
            "connections"
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "description": "List the affiliate-customer connections in your program",
          "parameters": [
            {
              "in": "query",
              "name": "since_id",
              "description": "Returns result after this ID",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "affiliate_id",
              "description": "Filters results to match this affiliate ID",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "customer_email",
              "description": "Filters results to match this customer email address",
              "type": "string"
            }
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "connections": {
                        "type": "array",
                        "items": {
                          "$ref": "#/components/schemas/Connections"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "connections"
          ],
          "description": "Creates connections between your affiliates and customers",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "connections": {
                      "type": "array",
                      "description": "List of connections to create",
                      "items": {
                        "type": "object",
                        "properties": {
                          "customer": {
                            "type": "object",
                            "properties": {
                              "name": {
                                "type": "string",
                                "description": "Name of the customer"
                              },
                              "email": {
                                "type": "string",
                                "description": "Email address of the customer",
                                "required": true
                              }
                            }
                          },
                          "affiliate": {
                            "type": "object",
                            "properties": {
                              "affiliate_id": {
                                "description": "ID of the affiliate to connect this customer with",
                                "type": "string",
                                "required": true
                              }
                            }
                          }
                        }
                      }
                    },
                    "overwrite": {
                      "type": "boolean",
                      "description": "Overwrite the already existing connections"
                    }
                  },
                  "example": {
                    "connections": [
                      {
                        "customer": {
                          "name": "John Doe",
                          "email": "jdoe@example.com"
                        },
                        "affiliate": {
                          "id": "1234"
                        }
                      }
                    ],
                    "overwrite": false
                  }
                }
              },
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "connections": {
                      "type": "array",
                      "description": "List of connections to create",
                      "items": {
                        "type": "object",
                        "properties": {
                          "customer": {
                            "type": "object",
                            "properties": {
                              "name": {
                                "type": "string",
                                "description": "Name of the customer"
                              },
                              "email": {
                                "type": "string",
                                "description": "Email address of the customer",
                                "required": true
                              }
                            }
                          },
                          "affiliate": {
                            "type": "object",
                            "properties": {
                              "affiliate_id": {
                                "description": "ID of the affiliate to connect this customer with",
                                "type": "string",
                                "required": true
                              }
                            }
                          }
                        }
                      }
                    },
                    "overwrite": {
                      "type": "boolean",
                      "description": "Overwrite the already existing connections"
                    }
                  },
                  "example": {
                    "connections": [
                      {
                        "customer": {
                          "name": "John Doe",
                          "email": "jdoe@example.com"
                        },
                        "affiliate": {
                          "id": "1234"
                        }
                      }
                    ],
                    "overwrite": false
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "created": {
                        "type": "number"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/connections/{id}": {
        "delete": {
          "tags": [
            "connections"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "path",
              "name": "id",
              "description": "Connection ID to delete",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "id": {
                        "type": "number"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/user/login": {
        "post": {
          "tags": [
            "user"
          ],
          "summary": "Log in to get the access token",
          "description": "Login to your affiliate account with your email address and password to retrieve your access token. You can use this access token to query data regarding your affiliate account",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "requestBody": {
            "content": {
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "required": [
                    "email",
                    "password"
                  ],
                  "properties": {
                    "email": {
                      "type": "string"
                    },
                    "password": {
                      "type": "string"
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "access_token": {
                        "type": "string",
                        "description": "Access token to query other /user/ endpoints"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/user/sites": {
        "get": {
          "summary": "List of stores the user is enrolled in",
          "tags": [
            "user"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "user": []
            },
            {
              "userAdmin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "status",
              "description": "Filters stores by status",
              "schema": {
                "type": "string",
                "enum": [
                  "approved",
                  "pending",
                  "blocked"
                ]
              }
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "name",
                    "logo",
                    "website",
                    "status",
                    "currency",
                    "affiliate_portal",
                    "ref_code",
                    "referral_link",
                    "coupon"
                  ]
                }
              },
              "explode": false,
              "required": true
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "id": {
                        "type": "string",
                        "description": "ID of the site"
                      },
                      "name": {
                        "type": "string",
                        "description": "Name of the store"
                      },
                      "logo": {
                        "type": "string",
                        "description": "URL Of the store logo"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/user": {
        "get": {
          "summary": "Get information about logged in user profile",
          "tags": [
            "user"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "user": []
            },
            {
              "userAdmin": []
            }
          ]
        },
        "post": {
          "tags": [
            "user"
          ],
          "summary": "Update your profile",
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "user": []
            },
            {
              "userAdmin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "id": {
                        "type": "number"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/user/stats/aggregate": {
        "get": {
          "summary": "Aggregates of user stats",
          "tags": [
            "user"
          ],
          "security": [
            {
              "user": []
            },
            {
              "userAdmin": []
            }
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "query",
              "name": "site_ids",
              "description": "Comma separated list of sites for which to return result for"
            },
            {
              "in": "query",
              "name": "start_time",
              "description": "Start date from where to aggregate results"
            },
            {
              "in": "query",
              "name": "end_time",
              "description": "End date up until where to aggregate results"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "total_sales",
                    "other_commission_earned",
                    "revenue_generated",
                    "sale_commission_earned",
                    "commission_paid"
                  ]
                }
              },
              "required": true,
              "explode": false
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "data": {
                        "type": "object",
                        "properties": {
                          "site_id": {
                            "type": "number"
                          },
                          "total_sales": {
                            "type": "number"
                          },
                          "currency": {
                            "type": "string"
                          },
                          "sale_commission_earned": {
                            "type": "number"
                          },
                          "revenue_generated": {
                            "type": "number"
                          },
                          "commission_paid": {
                            "type": "number"
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/user/feed/orders": {
        "get": {
          "summary": "Feed of user orders",
          "tags": [
            "user"
          ],
          "security": [
            {
              "user": []
            },
            {
              "userAdmin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "site_ids",
              "description": "Comma separated list of sites for which to return result for"
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Show orders after this order id"
            },
            {
              "in": "query",
              "name": "max_id",
              "description": "Show orders before this order id"
            },
            {
              "in": "query",
              "name": "created_at_max",
              "description": "Show orders older than this date"
            },
            {
              "in": "query",
              "name": "created_at_min",
              "description": "Show orders newer than this date"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return.",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "number",
                    "total",
                    "subtotal",
                    "line_items",
                    "commission",
                    "created_at",
                    "currency",
                    "site_id",
                    "sub_id",
                    "conversion_details"
                  ]
                }
              },
              "explode": false,
              "required": true
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "orders": {
                        "type": "object"
                      },
                      "limit": {
                        "type": "integer"
                      },
                      "offset": {
                        "type": "integer"
                      },
                      "count": {
                        "type": "integer"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/user/feed/payouts": {
        "get": {
          "summary": "Feed of user payouts",
          "tags": [
            "user"
          ],
          "security": [
            {
              "user": []
            },
            {
              "userAdmin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "site_ids",
              "description": "Comma separated list of sites for which to return result for"
            },
            {
              "in": "query",
              "name": "start_time",
              "description": "Start date from where to aggregate results"
            },
            {
              "in": "query",
              "name": "end_time",
              "description": "End date up until where to aggregate results"
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Returns result after this ID",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ]
        }
      },
      "/user/feed/products": {
        "get": {
          "summary": "Feed of products available for promotion. It can take upto 24 hours for the feed of newly enrolled stores to become available to the affiliate",
          "tags": [
            "user"
          ],
          "security": [
            {
              "user": []
            },
            {
              "userAdmin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ]
        }
      },
      "/user/feed/rewards": {
        "get": {
          "summary": "Feed of user rewards",
          "tags": [
            "user"
          ],
          "security": [
            {
              "user": []
            },
            {
              "userAdmin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "site_ids",
              "description": "Comma separated list of sites for which to return result for"
            },
            {
              "in": "query",
              "name": "start_time",
              "description": "Start date from where to aggregate results"
            },
            {
              "in": "query",
              "name": "end_time",
              "description": "End date up until where to aggregate results"
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Returns result after this ID",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ]
        }
      },
      "/user/feed/traffic": {
        "get": {
          "summary": "Feed of user traffic",
          "tags": [
            "user"
          ],
          "security": [
            {
              "user": []
            },
            {
              "userAdmin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "site_ids",
              "description": "Comma separated list of sites for which to return result for"
            },
            {
              "in": "query",
              "name": "start_time",
              "description": "Start date from where to aggregate results"
            },
            {
              "in": "query",
              "name": "end_time",
              "description": "End date up until where to aggregate results"
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Returns result after this ID",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "traffic": {
                        "type": "object"
                      },
                      "limit": {
                        "type": "integer"
                      },
                      "offset": {
                        "type": "integer"
                      },
                      "count": {
                        "type": "integer"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/user/commissions": {
        "get": {
          "summary": "Get commission structure",
          "tags": [
            "user"
          ],
          "security": [
            {
              "user": []
            },
            {
              "userAdmin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "site_ids",
              "description": "Comma separated list of sites for which to return result for"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "standard": {
                        "type": "object"
                      },
                      "special": {
                        "type": "array"
                      },
                      "royalties": {
                        "type": "array"
                      },
                      "mlm": {
                        "type": "object"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/groups": {
        "get": {
          "tags": [
            "groups"
          ],
          "summary": "Lists created groups",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "id",
              "description": "Retrieves only groups specified by comma-separated list of group IDs"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return.",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "name",
                    "commissions",
                    "config"
                  ]
                }
              },
              "explode": false,
              "required": true
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "groups": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "name": {
                              "type": "string",
                              "description": "Name of the group"
                            },
                            "commissions": {
                              "type": "object",
                              "description": "The commission structure for this group"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "groups"
          ],
          "summary": "Create new group",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "group": {
                      "type": "object",
                      "properties": {
                        "name": {
                          "type": "string",
                          "description": "Name of the group"
                        },
                        "commissions": {
                          "type": "object",
                          "description": "Commission structure for this group",
                          "properties": {
                            "standard": {
                              "type": "object",
                              "properties": {
                                "commission_type": {
                                  "type": "string",
                                  "enum": [
                                    "percentage",
                                    "flat_rate",
                                    "fixed_amount_on_order"
                                  ]
                                },
                                "commission_value": {
                                  "type": "integer"
                                }
                              }
                            },
                            "specific": {
                              "type": "array",
                              "description": "List of products/collections which have a different commission than the default commission",
                              "items": {
                                "type": "object",
                                "properties": {
                                  "commission_type": {
                                    "type": "string",
                                    "enum": [
                                      "percentage",
                                      "flat_rate"
                                    ]
                                  },
                                  "commission_value": {
                                    "type": "integer"
                                  },
                                  "product": {
                                    "type": "object",
                                    "properties": {
                                      "id": {
                                        "type": "string"
                                      },
                                      "name": {
                                        "type": "string"
                                      }
                                    }
                                  },
                                  "collection": {
                                    "type": "object",
                                    "properties": {
                                      "id": {
                                        "type": "string"
                                      },
                                      "name": {
                                        "type": "string"
                                      }
                                    }
                                  }
                                }
                              }
                            },
                            "mlm": {
                              "type": "array",
                              "description": "MLM commission rates for every level",
                              "items": {
                                "type": "object",
                                "properties": {
                                  "level": {
                                    "type": "integer",
                                    "description": "The level on which this commission rate applies to"
                                  },
                                  "commission_type": {
                                    "type": "string",
                                    "enum": [
                                      "percentage",
                                      "flat_rate"
                                    ]
                                  },
                                  "commission_value": {
                                    "type": "integer"
                                  }
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/groups/{id}": {
        "parameters": [
          {
            "in": "path",
            "name": "id",
            "description": "ID of the group to update",
            "required": true
          }
        ],
        "put": {
          "tags": [
            "groups"
          ],
          "summary": "Updates a group",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "group": {
                      "type": "object",
                      "properties": {
                        "name": {
                          "type": "string",
                          "description": "Name of the group"
                        },
                        "commissions": {
                          "type": "object",
                          "description": "Commission structure for this group",
                          "properties": {
                            "standard": {
                              "type": "object",
                              "properties": {
                                "commission_type": {
                                  "type": "string",
                                  "enum": [
                                    "percentage",
                                    "flat_rate",
                                    "fixed_amount_on_order"
                                  ]
                                },
                                "commission_value": {
                                  "type": "integer"
                                }
                              }
                            },
                            "specific": {
                              "type": "array",
                              "description": "List of products/collections which have a different commission than the default commission",
                              "items": {
                                "type": "object",
                                "properties": {
                                  "commission_type": {
                                    "type": "string",
                                    "enum": [
                                      "percentage",
                                      "flat_rate"
                                    ]
                                  },
                                  "commission_value": {
                                    "type": "integer"
                                  },
                                  "product": {
                                    "type": "object",
                                    "properties": {
                                      "id": {
                                        "type": "string"
                                      },
                                      "name": {
                                        "type": "string"
                                      }
                                    }
                                  },
                                  "collection": {
                                    "type": "object",
                                    "properties": {
                                      "id": {
                                        "type": "string"
                                      },
                                      "name": {
                                        "type": "string"
                                      }
                                    }
                                  }
                                }
                              }
                            },
                            "mlm": {
                              "type": "array",
                              "description": "MLM commission rates for every level",
                              "items": {
                                "type": "object",
                                "properties": {
                                  "level": {
                                    "type": "integer",
                                    "description": "The level on which this commission rate applies to"
                                  },
                                  "commission_type": {
                                    "type": "string",
                                    "enum": [
                                      "percentage",
                                      "flat_rate"
                                    ]
                                  },
                                  "commission_value": {
                                    "type": "integer"
                                  }
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "id": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "delete": {
          "tags": [
            "groups"
          ],
          "summary": "Deletes a group",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ]
        }
      },
      "/admin/groups/{id}/members": {
        "parameters": [
          {
            "in": "path",
            "name": "id",
            "description": "ID of the group",
            "required": true
          }
        ],
        "get": {
          "tags": [
            "groups"
          ],
          "summary": "Retrieves a list of affiliate ids who are part of this group",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "members": {
                        "type": "array",
                        "items": {
                          "type": "integer"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "groups"
          ],
          "summary": "Add a member to this group",
          "description": "Adds the list of members to this group",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "members": {
                      "type": "array",
                      "items": {
                        "type": "integer",
                        "summary": "Affiliate IDs to set in this group"
                      }
                    }
                  }
                },
                "example": {
                  "members": [
                    1001,
                    13217,
                    39404
                  ]
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "success": {
                        "type": "boolean"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "put": {
          "tags": [
            "groups"
          ],
          "summary": "Set affiliates who are in this group",
          "description": "This will replace all the members in this group with the new members",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "members": {
                      "type": "array",
                      "items": {
                        "type": "integer",
                        "summary": "Affiliate IDs to set in this group"
                      }
                    }
                  }
                },
                "example": {
                  "members": [
                    1001,
                    13217,
                    39404
                  ]
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "members": {
                        "type": "array",
                        "items": {
                          "type": "integer"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/groups/{id}/members/{affiliate_id}": {
        "parameters": [
          {
            "in": "path",
            "name": "id",
            "description": "ID of the group",
            "required": true
          },
          {
            "in": "path",
            "name": "affiliate_id",
            "description": "Affiliate ID to remove from the group",
            "required": true
          }
        ],
        "delete": {
          "tags": [
            "groups"
          ],
          "summary": "Removes the affiliate from the group.",
          "description": "Removes the affiliate from the group. The affiliate now will be put in the default group.",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "success": {
                        "type": "boolean"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/creatives": {
        "get": {
          "tags": [
            "creatives"
          ],
          "summary": "Lists media assets uploaded in the Creatives section of the admin panel",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "id",
              "description": "Retrieves only media assets specified by comma-separated list of creative IDs"
            },
            {
              "in": "query",
              "name": "category",
              "description": "Retrieves media assets specified by a comma-separated list of categories"
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit the number of results"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return.",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "category",
                    "url",
                    "thumbnail",
                    "description",
                    "link",
                    "name",
                    "size",
                    "width",
                    "height",
                    "mime"
                  ]
                }
              },
              "explode": false,
              "required": true
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "creatives": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "number"
                            },
                            "category": {
                              "type": "string"
                            },
                            "url": {
                              "type": "string"
                            },
                            "thumbnail": {
                              "type": "string"
                            },
                            "description": {
                              "type": "string"
                            },
                            "link": {
                              "type": "string"
                            },
                            "name": {
                              "type": "string",
                              "description": "Name of the file"
                            },
                            "size": {
                              "type": "number",
                              "description": "File size in bytes"
                            },
                            "width": {
                              "type": "number",
                              "description": "Image width in pixels (will be null for other file types)"
                            },
                            "height": {
                              "type": "number",
                              "description": "Image height in pixels (will be null for other file types)"
                            },
                            "mime": {
                              "type": "mime",
                              "description": "mime-type of this file"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/creatives/preSignedUrl": {
        "post": {
          "tags": [
            "creatives"
          ],
          "summary": "Returns a pre-signed URL to upload media asset",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "name": {
                      "type": "string",
                      "description": "Name of the file to be uploaded"
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {}
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/traffic": {
        "get": {
          "tags": [
            "traffic"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "description": "Retrieves affiliate traffic to the site",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "query",
              "name": "affiliate_id",
              "description": "Retrieve only the visits specified by a comma-separated list of Affiliate IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Show orders after the specified order ID",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "created_at_max",
              "schema": {
                "type": "string"
              },
              "description": "Show orders created at or before date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "created_at_min",
              "schema": {
                "type": "string"
              },
              "description": "Show orders created at or after date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return. See Affiliate Schema to get list of available fields",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "affiliate_id",
                    "landing_page",
                    "referring_page",
                    "ip_address",
                    "user_agent",
                    "created_at"
                  ]
                }
              },
              "explode": false,
              "required": true
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results. Max 1000",
              "type": "integer",
              "max": 1000
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "traffic": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "integer",
                              "description": "Visit ID"
                            },
                            "affiliate_id": {
                              "type": "number",
                              "description": "ID of the affiliate who brought this visit"
                            },
                            "referring_page": {
                              "type": "string",
                              "description": "URL of the page from where the click originated. Can be empty if no referrer is set"
                            },
                            "landing_page": {
                              "type": "string",
                              "description": "URL of the landing page. Can be empty if the landing page is home page"
                            },
                            "ip_address": {
                              "type": "string",
                              "description": "IPV4 address of the visitor or customer"
                            },
                            "user_agent": {
                              "type": "string",
                              "description": "User agent of the visitor's device"
                            },
                            "created_at": {
                              "type": "string",
                              "description": "The date when visitor came"
                            }
                          },
                          "example": {
                            "id": 1,
                            "affiliate_id": 1,
                            "landing_page": "https://yourstore.com",
                            "referring_page": "https://google.com",
                            "ip_address": "127.0.0.1",
                            "user_agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/72.0.3626.109 Safari/537.36",
                            "created_at": "Wed Feb 18 2026 07:38:43 GMT+0100 (Central European Standard Time)"
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/store/config": {
        "get": {
          "tags": [
            "config"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "description": "Retrieves configuration for the store",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "query",
              "name": "keys",
              "description": "Comma separated list of keys to return.",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "site_id",
                    "store_name",
                    "default_currency",
                    "website",
                    "discount_properties",
                    "available_payout_options"
                  ]
                }
              },
              "explode": false,
              "required": true
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "config": {
                        "type": "object"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "patch": {
          "tags": [
            "config"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "description": "Updates configuration of the store",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "key": {
                      "type": "string"
                    }
                  },
                  "example": {
                    "config": {
                      "store_name": "ABC STORE",
                      "default_currency": "USD"
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "schema": {
                      "type": "object",
                      "properties": {
                        "key": {
                          "type": "string"
                        }
                      },
                      "example": {
                        "config": {
                          "store_name": "ABC STORE",
                          "default_currency": "USD"
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/store/upgrade": {
        "get": {
          "tags": [
            "config"
          ],
          "summary": "To upgrade the app to a subscription plan.",
          "description": "This endpoint returns a confirmationUrl. Open this URL in your browser to complete the payment flow",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "test",
              "description": "Optional parameter if you want to test the upgrade flow",
              "schema": {
                "type": "string",
                "enum": [
                  1
                ]
              }
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "confirmationUrl": {
                        "type": "string",
                        "description": "The URL of the page where the merchant completes the payment flow"
                      }
                    }
                  },
                  "example": {
                    "confirmationUrl": "https://www.paypal.com/webapps/billing/subscriptions?ba_token=BA-4FW20531X7339414V"
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/store/logs": {
        "get": {
          "tags": [
            "config"
          ],
          "summary": "Get admin event logs",
          "description": "Retrieves paginated admin event logs for the store. Requires super admin permissions.",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "min_id",
              "description": "Filter logs with ID greater than this value",
              "schema": {
                "type": "integer"
              }
            },
            {
              "in": "query",
              "name": "max_id",
              "description": "Filter logs with ID less than this value",
              "schema": {
                "type": "integer"
              }
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Number of records to skip",
              "schema": {
                "type": "integer"
              }
            },
            {
              "in": "query",
              "name": "page",
              "description": "Page number (1-indexed)",
              "schema": {
                "type": "integer"
              }
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "total": {
                        "type": "integer",
                        "description": "Total number of logs"
                      },
                      "limit": {
                        "type": "integer",
                        "description": "Number of logs per page"
                      },
                      "offset": {
                        "type": "integer",
                        "description": "Current offset"
                      },
                      "logs": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "integer"
                            },
                            "type": {
                              "type": "string"
                            },
                            "action": {
                              "type": "string"
                            },
                            "message": {
                              "type": "string"
                            },
                            "metadata": {
                              "type": "object"
                            },
                            "resource": {
                              "type": "string"
                            },
                            "user": {
                              "type": "object",
                              "properties": {
                                "affiliate_id": {
                                  "type": "integer"
                                },
                                "name": {
                                  "type": "string"
                                },
                                "email": {
                                  "type": "string"
                                }
                              }
                            },
                            "created_at": {
                              "type": "string",
                              "format": "date-time"
                            }
                          }
                        }
                      },
                      "error": {
                        "type": "string",
                        "description": "Error message if any"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/coupons": {
        "get": {
          "tags": [
            "coupons"
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "description": "List the coupon codes assigned in the app",
          "parameters": [
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "schema": {
                "type": "integer"
              }
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "schema": {
                "type": "integer"
              }
            },
            {
              "in": "query",
              "name": "affiliate_id",
              "description": "Filters results to match this affiliate ID",
              "schema": {
                "type": "integer"
              }
            },
            {
              "in": "query",
              "name": "code",
              "description": "Filters results to match this coupon code",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "type",
              "type": "string",
              "description": "Filters results to match this coupon type",
              "schema": {
                "type": "string",
                "enum": [
                  "referral",
                  "personal"
                ]
              }
            }
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "coupons": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "code": {
                              "type": "string"
                            },
                            "discount_type": {
                              "type": "string",
                              "enum": [
                                "percentage",
                                "free_shipping",
                                "fixed_amount"
                              ]
                            },
                            "discount_value": {
                              "type": "integer"
                            },
                            "affiliate_id": {
                              "type": "integer"
                            },
                            "type": {
                              "type": "string",
                              "enum": [
                                "referral",
                                "personal"
                              ]
                            }
                          }
                        }
                      }
                    }
                  },
                  "example": {
                    "coupons": [
                      {
                        "code": "JOHN10OFF",
                        "discount_type": "percentage",
                        "discount_value": 10,
                        "affiliate_id": 7,
                        "type": "referral"
                      }
                    ]
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/affiliate": {
        "get": {
          "tags": [
            "sdk"
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "sdk": []
            }
          ],
          "description": "Returns the affiliate public information",
          "parameters": [
            {
              "in": "query",
              "name": "ref_code",
              "description": "Returns affiliate by referral code",
              "type": "string"
            },
            {
              "in": "query",
              "name": "coupon",
              "description": "Returns affiliate by coupon code",
              "type": "string"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return. See Affiliate Schema to get list of available fields",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "profile_photo",
                    "coupon",
                    "name",
                    "first_name",
                    "last_name"
                  ]
                }
              },
              "explode": false,
              "required": true
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "affiliate": {
                        "type": "object",
                        "$ref": "#/components/schemas/Affiliate"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/track/visit": {
        "post": {
          "tags": [
            "sdk"
          ],
          "description": "Adds traffic data to the system",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "sdk": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "id": {
                      "type": "string",
                      "description": "A visit ID to send with subsequent requests to maintain the session. For first session pass id as null"
                    },
                    "location": {
                      "type": "string",
                      "description": "The landing page URL"
                    },
                    "ref": {
                      "description": "Referral code of this visit",
                      "type": "string",
                      "required": true
                    },
                    "referrer": {
                      "description": "Referring URL",
                      "type": "string"
                    }
                  }
                },
                "example": {
                  "id": "1231212",
                  "location": "https://mystore.com/landing.html?ref=abcxyz",
                  "referrer": null,
                  "ref": "abcxyz"
                }
              },
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "id": {
                      "type": "string",
                      "description": "A visit ID to send with subsequent requests to maintain the session. For first session pass id as null"
                    },
                    "location": {
                      "type": "string",
                      "description": "The landing page URL"
                    },
                    "ref": {
                      "description": "Referral code of this visit",
                      "type": "string",
                      "required": true
                    },
                    "referrer": {
                      "description": "Referring URL",
                      "type": "string"
                    }
                  }
                },
                "example": {
                  "id": "1231212",
                  "location": "https://mystore.com/landing.html?ref=abcxyz",
                  "referrer": null,
                  "ref": "abcxyz"
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "example": {
                      "id": "1231212",
                      "location": "https://mystore.com/landing.html?ref=abcxyz",
                      "referrer": null,
                      "ref": "abcxyz"
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/track/conversion": {
        "post": {
          "tags": [
            "sdk"
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "sdk": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "order": {
                      "type": "object",
                      "properties": {
                        "id": {
                          "type": "string"
                        },
                        "number": {
                          "type": "string"
                        },
                        "total": {
                          "type": "string"
                        },
                        "coupons": {
                          "type": "array",
                          "items": {
                            "type": "string"
                          }
                        }
                      }
                    },
                    "ref": {
                      "type": "string"
                    }
                  }
                },
                "example": {
                  "order": {
                    "id": "1001",
                    "number": "#1001",
                    "total": 1000,
                    "coupons": [
                      "EASY10OFF"
                    ]
                  },
                  "ref": "7hbas62nd89"
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "example": {}
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/track/lead": {
        "post": {
          "tags": [
            "sdk"
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "sdk": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "lead": {
                      "type": "object",
                      "properties": {
                        "email": {
                          "type": "string",
                          "description": "Email address of this customer/lead"
                        },
                        "name": {
                          "type": "string",
                          "description": "Name of the customer/lead"
                        },
                        "source": {
                          "type": "string",
                          "required": false,
                          "description": "Source of this lead"
                        },
                        "reference_id": {
                          "type": "string",
                          "description": "Optional identifier to attach with this lead"
                        }
                      }
                    },
                    "ref": {
                      "type": "string"
                    }
                  }
                },
                "example": {
                  "lead": {
                    "name": "John doe",
                    "email": "johndoe@example.com",
                    "reference_id": "1002",
                    "source": "signup_form"
                  },
                  "ref": "7hbas62nd89"
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "example": {}
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/login": {
        "post": {
          "tags": [
            "sdk"
          ],
          "summary": "Log in to the affiliate account for the access token",
          "description": "Login to the affiliate account with your email address and password to retrieve the access token. You can use this access token to query data regarding your affiliate account",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "sdk": []
            }
          ],
          "requestBody": {
            "content": {
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "required": [
                    "email",
                    "password"
                  ],
                  "properties": {
                    "email": {
                      "type": "string"
                    },
                    "password": {
                      "type": "string"
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "access_token": {
                        "type": "string",
                        "description": "Access token to query other /user/ endpoints"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/register": {
        "post": {
          "tags": [
            "sdk"
          ],
          "summary": "Sign up for a new user account in the program",
          "description": "Sign up for a new user account in the program. The required fields are the ones set in the signup form in the admin panel. If you have turned on recaptcha, use the site key 6Lf_jsQUAAAAAOLW40PpDXgZQDIjjnGldAE1fhYr for the recaptcha input and send the response in the form recaptcha_response field",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "sdk": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "required": [
                    "email",
                    "password"
                  ],
                  "properties": {
                    "name": {
                      "type": "string"
                    },
                    "email": {
                      "type": "string"
                    },
                    "password": {
                      "type": "string"
                    },
                    "date_of_birth": {
                      "type": "string",
                      "description": "Must be in format DD-MM-YYYY"
                    },
                    "first_name": {
                      "type": "string"
                    },
                    "last_name": {
                      "type": "string"
                    },
                    "instagram": {
                      "type": "string"
                    },
                    "facebook": {
                      "type": "string"
                    },
                    "twitter": {
                      "type": "string"
                    },
                    "snapchat": {
                      "type": "string"
                    },
                    "pinterest": {
                      "type": "string"
                    },
                    "tiktok": {
                      "type": "string"
                    },
                    "youtube": {
                      "type": "string"
                    },
                    "address_1": {
                      "type": "string"
                    },
                    "address_2": {
                      "type": "string"
                    },
                    "city": {
                      "type": "string"
                    },
                    "state": {
                      "type": "string"
                    },
                    "country": {
                      "type": "string"
                    },
                    "zip": {
                      "type": "string"
                    },
                    "company_name": {
                      "type": "string"
                    },
                    "tax_identification_number": {
                      "type": "string"
                    },
                    "recaptcha_response": {
                      "type": "string"
                    }
                  }
                },
                "example": {
                  "name": "John Doe",
                  "email": "johndoe@example.com",
                  "password": "johndoe"
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "access_token": {
                        "type": "string",
                        "description": "Access token to query other /sdk/user/ endpoints"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/forgot-password": {
        "post": {
          "tags": [
            "sdk"
          ],
          "summary": "Sends a password reset email to the affiliate account",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "sdk": []
            }
          ],
          "requestBody": {
            "content": {
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "required": [
                    "email"
                  ],
                  "properties": {
                    "email": {
                      "type": "string"
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "success": {
                        "type": "string",
                        "description": "This is always 1"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/send-verification-email": {
        "post": {
          "tags": [
            "sdk"
          ],
          "summary": "Sends verification link to the affiliate for verification of their email address",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "success": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/reset-password": {
        "post": {
          "tags": [
            "sdk"
          ],
          "summary": "Resets the password of the affiliate based on the supplied token",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "sdk": []
            }
          ],
          "requestBody": {
            "content": {
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "required": [
                    "email",
                    "reset_token",
                    "new_password"
                  ],
                  "properties": {
                    "email": {
                      "type": "string"
                    },
                    "reset_token": {
                      "type": "string"
                    },
                    "new_password": {
                      "type": "string"
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "access_token": {
                        "type": "string",
                        "description": "Access token to query other /sdk/user/ endpoints"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/update-password": {
        "post": {
          "tags": [
            "sdk"
          ],
          "summary": "Resets the password of the affiliate based on the supplied old password",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "requestBody": {
            "content": {
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "required": [
                    "current_password",
                    "new_password"
                  ],
                  "properties": {
                    "current_password": {
                      "type": "string"
                    },
                    "new_password": {
                      "type": "string"
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "access_token": {
                        "type": "string",
                        "description": "Access token to query other /sdk/user/ endpoints"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user": {
        "get": {
          "summary": "Get information about logged in affiliate's profile",
          "tags": [
            "sdk"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "user": {
                        "type": "object",
                        "properties": {
                          "first_name": {
                            "type": "string"
                          },
                          "last_name": {
                            "type": "string"
                          },
                          "name": {
                            "type": "string"
                          },
                          "facebook": {
                            "type": "string"
                          },
                          "twitter": {
                            "type": "string"
                          },
                          "instagram": {
                            "type": "string"
                          },
                          "tiktok": {
                            "type": "string"
                          },
                          "pinterest": {
                            "type": "string"
                          },
                          "email": {
                            "type": "string"
                          },
                          "ref_code": {
                            "type": "string"
                          },
                          "coupon": {
                            "type": "string"
                          },
                          "email_verified": {
                            "type": "boolean"
                          },
                          "id": {
                            "type": "number"
                          },
                          "payment_method": {
                            "type": "string"
                          },
                          "payment_details": {
                            "type": "object",
                            "properties": {
                              "paypal_email": {
                                "type": "string"
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "sdk"
          ],
          "summary": "Update the affiliate's profile",
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "name": {
                      "type": "string"
                    },
                    "first_name": {
                      "type": "string"
                    },
                    "last_name": {
                      "type": "string"
                    },
                    "date_of_birth": {
                      "type": "string"
                    },
                    "honorific": {
                      "type": "string"
                    },
                    "gender": {
                      "type": "string"
                    },
                    "facebook": {
                      "type": "string"
                    },
                    "twitter": {
                      "type": "string"
                    },
                    "instagram": {
                      "type": "string"
                    },
                    "website": {
                      "type": "string"
                    },
                    "city": {
                      "type": "string"
                    },
                    "state": {
                      "type": "string"
                    },
                    "country": {
                      "type": "string"
                    },
                    "zip": {
                      "type": "string"
                    },
                    "phone": {
                      "type": "string"
                    },
                    "address_1": {
                      "type": "string"
                    },
                    "address_2": {
                      "type": "string"
                    },
                    "company_name": {
                      "type": "string"
                    },
                    "tax_identification_number": {
                      "type": "string"
                    },
                    "extra_1": {
                      "type": "string"
                    },
                    "extra_2": {
                      "type": "string"
                    },
                    "extra_3": {
                      "type": "string"
                    },
                    "ref_code": {
                      "type": "string"
                    },
                    "comments": {
                      "type": "string"
                    },
                    "payment_details": {
                      "type": "object",
                      "properties": {
                        "paypal_email": {
                          "type": "string"
                        }
                      }
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {}
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/stats/aggregate": {
        "get": {
          "summary": "Aggregates of user stats",
          "tags": [
            "sdk"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "query",
              "name": "start_time",
              "description": "Start date from where to aggregate results"
            },
            {
              "in": "query",
              "name": "end_time",
              "description": "End date up until where to aggregate results"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "total_sales",
                    "other_commission_earned",
                    "revenue_generated",
                    "sale_commission_earned",
                    "commission_paid"
                  ]
                }
              },
              "required": true,
              "explode": false
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "data": {
                        "type": "object",
                        "properties": {
                          "total_sales": {
                            "type": "number"
                          },
                          "currency": {
                            "type": "string"
                          },
                          "sale_commission_earned": {
                            "type": "number"
                          },
                          "revenue_generated": {
                            "type": "number"
                          },
                          "commission_paid": {
                            "type": "number"
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/feed/orders": {
        "get": {
          "summary": "Feed of user orders",
          "tags": [
            "sdk"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "since_id",
              "description": "Show orders after this order id"
            },
            {
              "in": "query",
              "name": "max_id",
              "description": "Show orders before this order id"
            },
            {
              "in": "query",
              "name": "created_at_max",
              "description": "Show orders older than this date"
            },
            {
              "in": "query",
              "name": "created_at_min",
              "description": "Show orders newer than this date"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return.",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "number",
                    "total",
                    "subtotal",
                    "line_items",
                    "commission",
                    "created_at",
                    "currency",
                    "site_id",
                    "sub_id",
                    "conversion_details"
                  ]
                }
              },
              "explode": false,
              "required": true
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "orders": {
                        "type": "object"
                      },
                      "limit": {
                        "type": "integer"
                      },
                      "offset": {
                        "type": "integer"
                      },
                      "count": {
                        "type": "integer"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/feed/payouts": {
        "get": {
          "summary": "Feed of user payouts",
          "tags": [
            "sdk"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "start_time",
              "description": "Start date from where to aggregate results"
            },
            {
              "in": "query",
              "name": "end_time",
              "description": "End date up until where to aggregate results"
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Returns result after this ID",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {}
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/feed/products": {
        "get": {
          "summary": "Feed of products available for promotion. It can take upto 24 hours for the feed of newly enrolled stores to become available to the affiliate",
          "tags": [
            "sdk"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {}
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/feed/rewards": {
        "get": {
          "summary": "Feed of user rewards",
          "tags": [
            "sdk"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "start_time",
              "description": "Start date from where to aggregate results"
            },
            {
              "in": "query",
              "name": "end_time",
              "description": "End date up until where to aggregate results"
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Returns result after this ID",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {}
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/feed/traffic": {
        "get": {
          "summary": "Feed of user traffic",
          "tags": [
            "sdk"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "start_time",
              "description": "Start date from where to aggregate results"
            },
            {
              "in": "query",
              "name": "end_time",
              "description": "End date up until where to aggregate results"
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Returns result after this ID",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "traffic": {
                        "type": "object"
                      },
                      "limit": {
                        "type": "integer"
                      },
                      "offset": {
                        "type": "integer"
                      },
                      "count": {
                        "type": "integer"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/feed/transactions": {
        "get": {
          "summary": "Feed of user transactions",
          "tags": [
            "sdk"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "transactions": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "string"
                            },
                            "amount": {
                              "type": "number"
                            },
                            "startingBalance": {
                              "type": "number"
                            },
                            "endingBalance": {
                              "type": "number"
                            },
                            "created_at": {
                              "type": "string"
                            },
                            "entity_type": {
                              "type": "string",
                              "enum": [
                                "rewards",
                                "orders",
                                "payouts"
                              ]
                            },
                            "event_type": {
                              "type": "string",
                              "enum": [
                                "insert",
                                "delete",
                                "update"
                              ]
                            },
                            "metadata": {
                              "type": "object"
                            }
                          }
                        }
                      },
                      "count": {
                        "type": "integer"
                      }
                    },
                    "example": {
                      "transactions": [
                        {
                          "id": 1,
                          "amount": 10,
                          "startingBalance": 0,
                          "endingBalance": 10,
                          "entity_type": "orders",
                          "event_type": "insert",
                          "entity_id": 100
                        }
                      ],
                      "count": 1
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/payouts/pending": {
        "get": {
          "summary": "Get pending payout amount (and its breakdown)",
          "tags": [
            "sdk"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "pending": {
                        "type": "object",
                        "properties": {
                          "sale_earnings": {
                            "type": "number"
                          },
                          "other_earnings": {
                            "type": "number"
                          },
                          "paid_earnings": {
                            "type": "number"
                          },
                          "amount_pending": {
                            "type": "number"
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/creatives": {
        "get": {
          "summary": "Get creatives uploaded by the admin",
          "tags": [
            "sdk"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "creatives": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "number"
                            },
                            "category": {
                              "type": "string"
                            },
                            "url": {
                              "type": "string"
                            },
                            "description": {
                              "type": "string"
                            },
                            "product_link": {
                              "type": "string"
                            },
                            "width": {
                              "type": "number"
                            },
                            "height": {
                              "type": "number"
                            },
                            "filesize": {
                              "type": "number"
                            },
                            "created_at": {
                              "type": "string"
                            }
                          }
                        }
                      }
                    },
                    "example": {
                      "creatives": [
                        {
                          "id": "1",
                          "url": "https://creatives.goaffpro.com/1/1.jpg",
                          "thumbnail": "https://creatives.goaffpro.com/thumbnail/1/1.jpg",
                          "filesize": 102400,
                          "width": 400,
                          "height": 300,
                          "description": "Some description about this image",
                          "product_link": "https://yourstore.com/",
                          "created_at": "Mon Aug 22 2021 17:11:54"
                        }
                      ]
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/sdk/user/commissions": {
        "get": {
          "summary": "Get user's commission structure",
          "tags": [
            "sdk"
          ],
          "security": [
            {
              "user": []
            },
            {
              "sdk": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "standard": {
                        "type": "object",
                        "properties": {}
                      },
                      "special": {
                        "type": "array"
                      },
                      "royalties": {
                        "type": "array"
                      },
                      "mlm": {
                        "type": "object",
                        "properties": {}
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/public/sites": {
        "get": {
          "tags": [
            "public"
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "description": "Public store/website/program data feed of the merchants enrolled in the marketplace program",
          "parameters": [
            {
              "in": "query",
              "name": "site_ids",
              "description": "List of comma separated list of site_ids to restrict the data set to",
              "type": "string"
            },
            {
              "in": "query",
              "name": "currency",
              "description": "List of comma separated currencies to restrict the data set to",
              "type": "string"
            },
            {
              "in": "query",
              "name": "keyword",
              "description": "Any search keyword to find stores in a particular niche",
              "type": "string"
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results. Min:0, Max: 10000, Default: 10",
              "type": "integer",
              "min": 0,
              "max": 10000
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "sites": {
                        "type": "object"
                      },
                      "count": {
                        "type": "integer"
                      },
                      "limit": {
                        "type": "integer"
                      },
                      "offset": {
                        "type": "integer"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/public/products": {
        "get": {
          "tags": [
            "public"
          ],
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "description": "Public product data feed of the merchants enrolled in the marketplace program",
          "parameters": [
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results. Min:0, Max: 10000, Default: 10",
              "type": "integer",
              "min": 0,
              "max": 10000
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset from where to get the results",
              "type": "integer"
            },
            {
              "in": "query",
              "name": "site_ids",
              "description": "List of comma separated list of site_ids to restrict the data set to",
              "type": "string"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "products": {
                        "type": "object"
                      },
                      "count": {
                        "type": "integer"
                      },
                      "limit": {
                        "type": "integer"
                      },
                      "offset": {
                        "type": "integer"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/files": {
        "get": {
          "tags": [
            "files"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "description": "Retrieves affiliate files",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "query",
              "name": "affiliate_id",
              "description": "Retrieve only the files uploaded by/for affiliates specified by a comma-separated list of Affiliate IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "since_id",
              "description": "Show files after the specified file ID",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "created_at_max",
              "schema": {
                "type": "string"
              },
              "description": "Show files created at or before date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "created_at_min",
              "schema": {
                "type": "string"
              },
              "description": "Show files created at or after date (format: Sun Jul 26 2019 12:10:07 GMT+0530)"
            },
            {
              "in": "query",
              "name": "fields",
              "description": "Comma separated list of fields to return. See Affiliate Schema to get list of available fields",
              "schema": {
                "type": "array",
                "items": {
                  "type": "string",
                  "enum": [
                    "id",
                    "affiliate_id",
                    "name",
                    "url",
                    "title",
                    "description",
                    "type",
                    "size",
                    "metadata",
                    "created_at"
                  ]
                }
              },
              "explode": false,
              "required": true
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results. Max 1000",
              "type": "integer",
              "max": 1000
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset of the result",
              "type": "integer"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "files": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "integer",
                              "description": "File ID"
                            },
                            "affiliate_id": {
                              "type": "number",
                              "description": "ID of the affiliate"
                            },
                            "name": {
                              "type": "string",
                              "description": "File name of the uploaded file (eg. abc.jpg)"
                            },
                            "title": {
                              "type": "string",
                              "description": "User specified title for this file (eg. Photo of ABC)"
                            },
                            "description": {
                              "type": "string",
                              "description": "User specified description for this file"
                            },
                            "type": {
                              "type": "string",
                              "description": "Mime-type of this file (eg. image/jpeg)"
                            },
                            "size": {
                              "type": "number",
                              "description": "Size of this file in bytes"
                            },
                            "metadata": {
                              "type": "object",
                              "description": "Any associated metadata. Can be null",
                              "properties": {
                                "width": {
                                  "type": "number",
                                  "description": "Width of the image"
                                },
                                "height": {
                                  "type": "number",
                                  "description": "Height of the image"
                                }
                              }
                            },
                            "created_at": {
                              "type": "string",
                              "description": "The date when the file was uploaded"
                            }
                          },
                          "example": {
                            "id": 1,
                            "affiliate_id": 1,
                            "name": "filename.jpg",
                            "title": "File name",
                            "description": "Description for this file",
                            "size": 195773,
                            "type": "image/jpg",
                            "metadata": {
                              "width": 300,
                              "height": 400
                            },
                            "created_at": "Wed Feb 18 2026 07:38:43 GMT+0100 (Central European Standard Time)"
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/files/:id": {
        "delete": {
          "tags": [
            "files"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "description": "Delete file by ID",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "path",
              "name": "id",
              "description": "ID of the file to delete",
              "schema": {
                "type": "string"
              }
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "success": {
                        "type": "number"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/commissions": {
        "get": {
          "tags": [
            "commissions"
          ],
          "summary": "Returns a list of all the special commission rates",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "commissions": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "integer",
                              "description": "The commission ID"
                            },
                            "commission": {
                              "type": "object",
                              "description": "The commission rate",
                              "properties": {
                                "commission_type": {
                                  "type": "string",
                                  "enum": [
                                    "percentage",
                                    "flat_rate"
                                  ]
                                },
                                "commission_value": {
                                  "type": "integer"
                                }
                              }
                            },
                            "product": {
                              "type": "object",
                              "properties": {
                                "name": {
                                  "type": "string",
                                  "description": "Name of the product",
                                  "required": true
                                },
                                "id": {
                                  "type": "string",
                                  "description": "Store ID of this product",
                                  "required": true
                                }
                              }
                            },
                            "collection": {
                              "type": "object",
                              "properties": {
                                "name": {
                                  "type": "string",
                                  "description": "Name of the collection",
                                  "required": true
                                },
                                "id": {
                                  "type": "string",
                                  "description": "Store ID of this collection",
                                  "required": true
                                }
                              }
                            }
                          }
                        }
                      }
                    },
                    "example": {
                      "commissions": [
                        {
                          "id": "1",
                          "collection": {
                            "id": "42",
                            "name": "Special collection"
                          },
                          "commission": {
                            "commission_type": "percentage",
                            "commission_value": 10
                          }
                        }
                      ]
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "commissions"
          ],
          "summary": "Adds a special commission rate to the commission plan",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "product": {
                      "type": "object",
                      "properties": {
                        "name": {
                          "type": "string",
                          "description": "Name of the product",
                          "required": true
                        },
                        "id": {
                          "type": "string",
                          "description": "Store ID of this product",
                          "required": true
                        }
                      }
                    },
                    "collection": {
                      "type": "object",
                      "properties": {
                        "name": {
                          "type": "string",
                          "description": "Name of the collection",
                          "required": true
                        },
                        "id": {
                          "type": "string",
                          "description": "Store ID of this collection",
                          "required": true
                        }
                      }
                    },
                    "commission": {
                      "type": "object",
                      "description": "Commission structure",
                      "properties": {
                        "commission_type": {
                          "type": "string",
                          "enum": [
                            "percentage",
                            "flat_rate"
                          ]
                        },
                        "commission_value": {
                          "type": "integer"
                        }
                      }
                    },
                    "affiliate_id": {
                      "type": "integer",
                      "description": "ID of the affiliate to which the special commission rate applies. (Optional)"
                    }
                  },
                  "example": {
                    "collection": {
                      "id": "123",
                      "name": "Special Collection"
                    },
                    "commission": {
                      "commission_type": "percentage",
                      "commission_value": 10
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "schema": {
                      "type": "object",
                      "properties": {
                        "id": {
                          "type": "integer",
                          "description": "ID of the commission"
                        },
                        "product": {
                          "type": "object",
                          "properties": {
                            "name": {
                              "type": "string",
                              "description": "Name of the product",
                              "required": true
                            },
                            "id": {
                              "type": "string",
                              "description": "Store ID of this product",
                              "required": true
                            }
                          }
                        },
                        "collection": {
                          "type": "object",
                          "properties": {
                            "name": {
                              "type": "string",
                              "description": "Name of the collection",
                              "required": true
                            },
                            "id": {
                              "type": "string",
                              "description": "Store ID of this collection",
                              "required": true
                            }
                          }
                        },
                        "commission": {
                          "type": "object",
                          "description": "Commission structure",
                          "properties": {
                            "commission_type": {
                              "type": "string",
                              "enum": [
                                "percentage",
                                "flat_rate"
                              ]
                            },
                            "commission_value": {
                              "type": "integer"
                            }
                          }
                        },
                        "affiliate_id": {
                          "type": "integer",
                          "description": "ID of the affiliate to which the special commission rate applies. (Optional)"
                        }
                      },
                      "example": {
                        "id": 1,
                        "collection": {
                          "id": "123",
                          "name": "Special Collection"
                        },
                        "commission": {
                          "commission_type": "percentage",
                          "commission_value": 10
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/commissions/{id}": {
        "delete": {
          "tags": [
            "commissions"
          ],
          "summary": "Deletes the special commission rates from the commission plan",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "path",
              "name": "id",
              "description": "ID of the special commission rate",
              "required": true
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "schema": {
                      "type": "object",
                      "properties": {
                        "id": {
                          "type": "integer",
                          "description": "ID of the commission"
                        }
                      },
                      "example": {
                        "id": 1,
                        "success": 1
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/commissions/collections": {
        "get": {
          "tags": [
            "commissions"
          ],
          "summary": "Retrieves a list of collections present in the store",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "keyword",
              "description": "Retrieves the collections matching the keyword in their name"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "products": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "name": {
                              "type": "string",
                              "description": "The name of the collection"
                            },
                            "id": {
                              "type": "string",
                              "description": "The ID of the collection"
                            }
                          }
                        }
                      }
                    },
                    "example": {
                      "collections": [
                        {
                          "id": "42",
                          "name": "Special collection"
                        },
                        {
                          "id": "43",
                          "name": "Gift cards"
                        }
                      ]
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/commissions/products": {
        "get": {
          "tags": [
            "commissions"
          ],
          "summary": "Retrieves a list of products present in the store (upto 250 results)",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "parameters": [
            {
              "in": "query",
              "name": "keyword",
              "description": "Retrieves the products matching the keyword in their name"
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "products": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "name": {
                              "type": "string",
                              "description": "The name of the product"
                            },
                            "id": {
                              "type": "string",
                              "description": "The ID of the product"
                            }
                          }
                        }
                      }
                    },
                    "example": {
                      "collections": [
                        {
                          "id": "42",
                          "name": "Special collection"
                        },
                        {
                          "id": "43",
                          "name": "Gift cards"
                        }
                      ]
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/notifications/send/{template_name}": {
        "post": {
          "tags": [
            "notifications"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "description": "Send a notification to your affiliates",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "path",
              "name": "template_name",
              "required": true,
              "description": "Name of the template to send to the affiliate",
              "schema": {
                "type": "string",
                "enum": [
                  "partneraccountapproved",
                  "partneraccountblocked",
                  "partnerpayoutemail",
                  "partnersale",
                  "partnerinvitation"
                ]
              }
            }
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "affiliate_id": {
                      "type": "string",
                      "required": true,
                      "description": "ID of the affiliate to send the notification to"
                    },
                    "order_id": {
                      "type": "string",
                      "description": "ID of the order (if an order template is being sent)"
                    },
                    "payout_id": {
                      "type": "string",
                      "description": "ID of the payout (if a payout template is being sent)"
                    },
                    "html": {
                      "type": "string",
                      "description": "HTML content of the email"
                    },
                    "subject": {
                      "type": "string",
                      "description": "Subject of the email"
                    },
                    "to": {
                      "type": "string",
                      "description": "To email address"
                    },
                    "from": {
                      "type": "string",
                      "description": "From email address"
                    },
                    "replyTo": {
                      "type": "string",
                      "description": "Reply-to email address"
                    }
                  }
                }
              },
              "application/x-www-form-urlencoded": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "affiliate_id": {
                      "type": "string",
                      "required": true,
                      "description": "ID of the affiliate to send the notification to"
                    },
                    "order_id": {
                      "type": "string",
                      "description": "ID of the order (if an order template is being sent)"
                    },
                    "payout_id": {
                      "type": "string",
                      "description": "ID of the payout (if a payout template is being sent)"
                    },
                    "html": {
                      "type": "string",
                      "description": "HTML content of the email"
                    },
                    "subject": {
                      "type": "string",
                      "description": "Subject of the email"
                    },
                    "to": {
                      "type": "string",
                      "description": "To email address"
                    },
                    "from": {
                      "type": "string",
                      "description": "From email address"
                    },
                    "replyTo": {
                      "type": "string",
                      "description": "Reply-to email address"
                    }
                  }
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "success": {
                        "type": "number"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/notifications": {},
      "/admin/webhooks": {
        "get": {
          "tags": [
            "webhooks"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "description": "Gets webhooks",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "webhooks": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "id": {
                              "type": "integer",
                              "description": "ID of the webhook"
                            },
                            "topic": {
                              "type": "string",
                              "description": "Webhook topic"
                            },
                            "url": {
                              "type": "string",
                              "description": "URL of the webhooks"
                            }
                          },
                          "example": {
                            "id": 1,
                            "topic": "orders/after",
                            "url": "https://your-webhook-url.com"
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        },
        "post": {
          "tags": [
            "webhooks"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "description": "Create a new webhook",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "requestBody": {
            "content": {
              "application/json": {
                "schema": {
                  "type": "object",
                  "properties": {
                    "topic": {
                      "type": "string",
                      "description": "Webhook topic"
                    },
                    "url": {
                      "type": "string",
                      "description": "URL of the webhooks"
                    }
                  },
                  "required": [
                    "topic",
                    "url"
                  ]
                }
              }
            }
          },
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "id": {
                        "type": "number",
                        "description": "ID of the created webhook"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/webhooks/:id": {
        "delete": {
          "tags": [
            "webhooks"
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "description": "Delete webhook by ID",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "path",
              "name": "id",
              "description": "ID of the webhook to delete",
              "schema": {
                "type": "string"
              }
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "success": {
                        "type": "number"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
      "/admin/transactions": {
        "get": {
          "tags": [
            "transactions"
          ],
          "description": "Retrieves list of transactions from the transaction log",
          "consumes": [
            "application/json",
            "application/x-www-form-urlencoded"
          ],
          "produces": [
            "application/json"
          ],
          "parameters": [
            {
              "in": "query",
              "name": "id",
              "description": "Retrieve only transactions specified by a comma-separated list of transaction IDs",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "affiliate_id",
              "description": "Filter transactions by affiliate ID",
              "schema": {
                "type": "string"
              }
            },
            {
              "in": "query",
              "name": "type",
              "description": "Filter by entity type (comma-separated list)",
              "schema": {
                "type": "string",
                "enum": [
                  "ORDERS",
                  "REWARDS",
                  "PAYOUTS"
                ]
              }
            },
            {
              "in": "query",
              "name": "is_paid",
              "description": "Filter by payment status",
              "schema": {
                "type": "boolean"
              }
            },
            {
              "in": "query",
              "name": "limit",
              "description": "Limit number of results",
              "schema": {
                "type": "integer"
              }
            },
            {
              "in": "query",
              "name": "offset",
              "description": "Offset of results",
              "schema": {
                "type": "integer"
              }
            }
          ],
          "security": [
            {
              "admin": []
            }
          ],
          "responses": {
            "200": {
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "transactions": {
                        "type": "array",
                        "items": {
                          "type": "object",
                          "properties": {
                            "tx_id": {
                              "type": "integer"
                            },
                            "affiliate_id": {
                              "type": "integer"
                            },
                            "amount": {
                              "type": "number"
                            },
                            "currency": {
                              "type": "string"
                            },
                            "event_type": {
                              "type": "string"
                            },
                            "entity_type": {
                              "type": "string",
                              "enum": [
                                "ORDERS",
                                "REWARDS",
                                "PAYOUTS"
                              ]
                            },
                            "entity_id": {
                              "type": "string"
                            },
                            "is_paid": {
                              "type": "boolean"
                            },
                            "metadata": {
                              "type": "object"
                            },
                            "created_at": {
                              "type": "string",
                              "format": "date-time"
                            },
                            "startingBalance": {
                              "type": "number"
                            },
                            "endingBalance": {
                              "type": "number"
                            }
                          }
                        }
                      },
                      "count": {
                        "type": "integer"
                      },
                      "limit": {
                        "type": "integer"
                      },
                      "offset": {
                        "type": "integer"
                      }
                    }
                  }
                }
              }
            },
            "403": {
              "description": "Not authenticated",
              "content": {
                "application/json": {
                  "schema": {
                    "type": "object",
                    "properties": {
                      "error": {
                        "type": "string"
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    }
  },
  "customOptions": {}
};
  url = options.swaggerUrl || url
  var urls = options.swaggerUrls
  var customOptions = options.customOptions
  var spec1 = options.swaggerDoc
  var swaggerOptions = {
    spec: spec1,
    url: url,
    urls: urls,
    dom_id: '#swagger-ui',
    deepLinking: true,
    presets: [
      SwaggerUIBundle.presets.apis,
      SwaggerUIStandalonePreset
    ],
    plugins: [
      SwaggerUIBundle.plugins.DownloadUrl
    ],
    layout: "StandaloneLayout"
  }
  for (var attrname in customOptions) {
    swaggerOptions[attrname] = customOptions[attrname];
  }
  var ui = SwaggerUIBundle(swaggerOptions)

  if (customOptions.oauth) {
    ui.initOAuth(customOptions.oauth)
  }

  if (customOptions.authAction) {
    ui.authActions.authorize(customOptions.authAction)
  }

  window.ui = ui
}
