`webserver_example.py` is a sample webserver, written in Python using the Flask framework. It can be used in conjunction with the [HTTP POST Data Handler](https://github.com/immersivecognition/unity-experiment-framework/wiki/HTTP-POST-setup). It accepts incoming POST requests, and writes the data contained in the form to a file. It uses Basic HTTP authentication.

To use it, you need some packages:

```
pip install Flask Flask-HTTPAuth Flask-CORS
```
