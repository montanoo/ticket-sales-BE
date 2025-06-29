package com.example.skipasstickets

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.tooling.preview.Preview
import com.android.volley.Request
import com.android.volley.RequestQueue
import com.android.volley.toolbox.JsonArrayRequest
import com.android.volley.toolbox.JsonObjectRequest
import com.android.volley.toolbox.Volley
import com.example.skipasstickets.ui.theme.SkipassTicketsTheme

class EntriesActivity : ComponentActivity() {
    private var requestQueue: RequestQueue? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        requestQueue = Volley.newRequestQueue(this)
        val userId = intent.extras?.getInt("userId")
        val request = JsonArrayRequest(
            // NOTE: to get this to work, run your server with --urls="your_local_ip:port" param and change this to that ip and port
            Request.Method.GET,
            "http://192.168.1.19:5168/api/tickets/user/$userId/entries", null,
            { response ->
                var entries: ArrayList<String> = ArrayList()
                for (x in 0..<response.length())
                {
                    var entry = response.getJSONObject(x)
                    entries.add(entry.getString("scannedAt"))
                }

                setContent {
                    Entries(entries)
                }
            },
            { error ->
                setContent {
                    Text("Could not retrieve the entries history")
                }
            })
        requestQueue?.add(request)

        enableEdgeToEdge()
        setContent {
                Text("...")
        }
    }
}

@Composable
fun Entries(entries: ArrayList<String>)
{
    LazyColumn{
        items(entries){entry->
            Text(text = entry)
        }
    }
}

/*@Composable
fun Greeting(name: String, modifier: Modifier = Modifier) {
    Text(
        text = "Hello $name!",
        modifier = modifier
    )
}*/