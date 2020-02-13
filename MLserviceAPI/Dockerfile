FROM rocm/tensorflow

COPY requirements.txt /tmp/
RUN pip3 install -r /tmp/requirements.txt

WORKDIR /usr/app
COPY src .

EXPOSE 5000
CMD [ "python3", "app.py" ]