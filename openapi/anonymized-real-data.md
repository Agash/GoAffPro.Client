# Real Data (anonymized & cleaned) (provided by <https://github.com/WolfwithSword>)

```json
// /user/sites?status=approved&fields=id,name,logo,website,status,currency,affiliate_portal,ref_code,referral_link,coupon
{
  "sites": [
    {
      "id": 132230,
      "name": "UwU Market",
      "logo": "https://creatives.goaffpro.com/132230/files/NDpAegaysoPA.png",
      "website": "https://uwumarket.us/",
      "status": "approved",
      "currency": "USD",
      "affiliate_portal": "https://uwumarket.goaffpro.com",
      "ref_code": "REFCODE1",
      "referral_link": "https://uwumarket.us/?ref=REFCODE1",
      "coupon": null
    },
    {
      "id": 165328,
      "name": "GamerSupps.GG",
      "logo": "https://creatives.goaffpro.com/165328/files/uHUzvTlLJX7n.png",
      "website": "https://gamersupps.gg/",
      "status": "approved",
      "currency": "USD",
      "affiliate_portal": "https://gamersupps.goaffpro.com",
      "ref_code": "CODE1",
      "referral_link": "https://gamersupps.gg/?ref=CODE1",
      "coupon": {
        "code": "CODE1",
        "discount_value": "",
        "discount_type": "percentage",
        "can_change": false
      }
    }
  ],
  "count": 2
}
```

```json
// user/stats/aggregate
// values for money have been replaced
// values for total sales have been replaced
{
  "data": [
    {
      "store_name": "GamerSupps.GG",
      "website": "gamersupps.gg",
      "default_currency": "USD",
      "site_id": 165328,
      "currency": "USD",
      "commission_paid": "1000.23",
      "total_sales": 123, // count of sales
      "sale_commission_earned": "2000.567000",
      "revenue_generated": "5000.45"
    },
    {
      "store_name": "UwU Market",
      "website": "uwumarket.us",
      "default_currency": "USD",
      "site_id": 132230,
      "currency": "USD",
      "commission_paid": "1000.23",
      "total_sales": 456, // count of sales
      "sale_commission_earned": "2000.560700",
      "revenue_generated": "5000.45"
    }
  ]
}
```

```json
// user/commissions
// used uwumarket id

{
  "commissions": [
    {
      "standard": {
        "commission_type": "percentage",
        "commission_value": 0,
        "commission_on": "order"
      },
      "special": [
        {
          "id": 123456,
          "commission_value": "10.00000",
          "commission_type": "percentage",
          "collection": {}
        }
      ],
      "royalties": [
        {
          "id": 234567,
          "commission_value": "21.50",
          "commission_type": "percentage",
          "collection": {
            "id": 112233445566,
            "name": "Affiliate_Name"
          }
        }
      ]
    }
  ]
}
```

```json
// users/feed/orders

{
  "orders": [
    {
      "site_id": 132230, // uwumarket
      "id": 12345678,
      "number": "#100001",
      "subtotal": "40.72",
      "total": "58.00",
      "sub_id": null,
      "conversion_details": null,
      "commission": "8.754800",
      "currency": "USD",
      "created_at": "2026-02-24T11:57:26.000Z",
      "website": "uwumarket.us",
      "store_name": "UwU Market",
      "status": "new",
      "line_items": [
        {
          "id": 20000000000000,
          "product_id": 2222222222222,
          "variation_id": 23333333333333,
          "sku": "SKU_STRING",
          "vendor": "UwU Market",
          "name": "Product Name",
          "quantity": 1,
          "refund_quantity": 0,
          "price": "40.72",
          "total_discount": 0,
          "total_price": 40.72,
          "total_tax": 0,
          "gift_card_amount_used": 0,
          "gift_card_percentage": 0,
          "total": 40.72,
          "exclude_discounts": true,
          "commission": 8.7548,
          "affiliate_id": 99999999,
          "commission_value": "21.50",
          "commission_type": "percentage"
        }
      ]
    },
    {
      "site_id": 132230,
      "id": 12345999,
      "number": "#100000",
      "subtotal": "85.09",
      "total": "173.24",
      "sub_id": null,
      "conversion_details": null,
      "commission": "18.294350",
      "currency": "USD",
      "created_at": "2026-01-14T23:59:03.000Z",
      "website": "uwumarket.us",
      "store_name": "UwU Market",
      "status": "approved",
      "line_items": [
        // I removed many items from this order, so amounts above may not add up
        {
          "id": 10000000000000,
          "product_id": 2222222222220,
          "variation_id": 2222222222222,
          "sku": "SKU_STRING",
          "vendor": "UwU Market",
          "name": "Product Name",
          "quantity": 1,
          "refund_quantity": 0,
          "price": "15.23",
          "total_discount": 0,
          "total_price": 15.23,
          "total_tax": 0,
          "gift_card_amount_used": 0,
          "gift_card_percentage": 0,
          "total": 15.23,
          "exclude_discounts": true,
          "commission": 3.27445,
          "affiliate_id": 99999999,
          "commission_value": "21.50",
          "commission_type": "percentage"
        },
        {
          "id": 10000000000005,
          "product_id": 2222222222000,
          "variation_id": 2222222111111,
          "sku": "SKU_STRING",
          "vendor": "UwU Market",
          "name": "Product Name 2",
          "quantity": 1,
          "refund_quantity": 0,
          "price": "13.14",
          "total_discount": 0,
          "total_price": 13.14,
          "total_tax": 0,
          "gift_card_amount_used": 0,
          "gift_card_percentage": 0,
          "total": 13.14,
          "exclude_discounts": true,
          "commission": 2.8251,
          "affiliate_id": 99999999,
          "commission_value": "21.50",
          "commission_type": "percentage"
        }
      ]
    }
  ],
  "count": 2,
  "offset": 0,
  "limit": 2
}
```

```json
// user/feed/traffic
{
  "traffic": [
    {
      "id": 3333333333,
      "landing_page": null,
      "user_agent": null,
      "ip_address": "IPV6_ADDR",
      "created_at": "2026-02-24T12:00:31.000Z",
      "order_id": 33333333,
      "sub_id": null
    },
    {
      "id": 2222222222,
      "landing_page": null,
      "user_agent": null,
      "ip_address": "IPV6_ADDR",
      "created_at": "2026-02-18T06:03:22.000Z",
      "order_id": 22222222,
      "sub_id": null
    },
    {
      "id": 1111111111,
      "landing_page": null,
      "user_agent": null,
      "ip_address": "IPV4_ADDR",
      "created_at": "2026-02-18T03:39:33.000Z",
      "order_id": 11111111,
      "sub_id": null
    }
  ],
  "count": 3,
  "limit": 3,
  "offset": 0
}
```
