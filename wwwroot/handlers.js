fetch('/login')
.then(chk => {
    if(chk.ok)
        chk.text().then((chk_result) => {
            document.styleSheets[0].cssRules[0].style.display = '';
            document.querySelector('#login-button').style.display = 'none';
            document.querySelector('#greeting').innerHTML = `Здравствуй, ${chk_result}`;
        });;
})


const addButtonElem = document.querySelector('#add-student-button');
addButtonElem.addEventListener('click', () => {
if(document.forms[0].dataset.id != '-1')
{
    document.forms[0].dataset.id = '-1';
    document.forms[0].reset();
    document.querySelector('#confirm-button').value = 'Добавить';
}
else if(document.forms[0].style.display == 'none')
    document.forms[0].style.display = '';
else
    document.forms[0].style.display = 'none';
});

const closeButtonElem = document.querySelector('#close-form-button');
closeButtonElem.addEventListener('click', () => {
    if(document.forms[0].dataset.id != '-1')
    {
        document.forms[0].dataset.id = '-1';
        document.forms[0].reset();
        document.querySelector('#confirm-button').value = 'Добавить';
    }
    document.forms[0].style.display = 'none';
});

document.forms[0].onsubmit = () =>{
    const fio = document.querySelector('#student-fio');
    const spec = document.querySelector('#student-spec');
    const course = document.querySelector('#student-course');
    const num = document.querySelector('#student-number');

    if(fio.value == '' || spec.value == '' || num.value == '')
    {
        alert('Введите все необходимые данные');
        return false;
    }

    if(document.forms[0].dataset.id == '-1')
    {    
        fetch('/last-id')
        .then(res => res.text())
        .then(result =>{
            const newRow = document.createElement('tr');
            newRow.innerHTML = `<td>${result}</td>
                                <td>${fio.value}</td>
                                <td>${course.value}</td>
                                <td>${spec.value}</td>
                                <td>${num.value}</td>
                                <td><input type="button" value="Изменить" 
                                    data-id="${result}" onclick="EditHandler(this)"/></td>
                                <td><input type="checkbox" data-id="${result}"/></td>`;
            document.querySelector('#table-body').append(newRow);
            fetch('/students', 
            {
                method: 'PUT', 
                headers: {
                    'Content-Type': 'application/json;charset=utf-8'
                },
                body: JSON.stringify({id: +result, 
                    fio: fio.value,
                    course: +course.value,
                    spec: spec.value,
                    number: num.value})
            });
            document.forms[0].reset();
        });
    }
    else
    {
        fetch('/students', 
        {
            method: 'POST', 
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify({id: +document.forms[0].dataset.id, 
                fio: fio.value,
                course: +course.value,
                spec: spec.value,
                number: num.value})
        });

        const rows = document.querySelector('#table-body').querySelectorAll('tr');
        let i = 0;
        for(; i < rows.length; i++)
        {
            console.log(rows[i].querySelector('td').innerHTML);
            if(rows[i].querySelector('td').innerHTML == document.forms[0].dataset.id)
                break;
        }

        const cells = rows[i].querySelectorAll('td');
        cells[1].innerHTML = fio.value;
        cells[2].innerHTML = course.value;
        cells[3].innerHTML = spec.value;
        cells[4].innerHTML = num.value;

        document.forms[0].reset();
        document.forms[0].dataset.id = '-1';
        document.querySelector('#confirm-button').value = 'Добавить';
    }
    return false;
};

const deleteButtonElem = document.querySelector('#delete-student-button');
deleteButtonElem.addEventListener('click', () =>{
    const a = [];
    const rows = document.querySelector('#table-body').querySelectorAll('tr');
    for(let i = 0; i < rows.length; i++)
    {
        const e = rows[i].querySelectorAll('input')[1];
        if(e.checked)
        {
            a.push(+e.dataset.id);
            rows[i].remove();
        }
    }

    if(a.length == 0)
    {
        alert('Не выбрано ни одной записи');
        return;
    }
    if(!confirm("Вы уверены, что хотите удалить выбранные строки?"))
        return;
    fetch('/students', 
        {
            method: 'DELETE', 
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(a)
        });
    
});

const loginForm = document.querySelector('#login-form');
const loginButton = document.querySelector('#login-button');
loginButton.addEventListener('click', () =>{
    loginForm.style.display = '';
    loginForm.querySelector('#login-textbox').focus();
});
loginForm.onsubmit = () =>{
    fetch('/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded;charset=utf-8'
        },
        body: "login=" + loginForm.querySelector('#login-textbox').value 
        + "&password=" + loginForm.querySelector('#password-textbox').value
    })
    .then(res => {
        if(res.ok && res.redirected){
            setTimeout(() => location.href = res.url, 250);
        }
        else{
            let t = loginForm.querySelector('#error-text');
            console.log(t);
            if(!t){
                let elem = document.createElement('p');
                elem.id = 'error-text';
                console.log('asdsad');
                elem.style.color = 'red';
                elem.innerHTML = 'Неверные имя пользователя и/или пароль';
                loginForm.querySelector('#password-textbox').after(elem);
            }
        }
    });
    return false;
};
document.querySelector('#logout-button').onclick = () =>{
    fetch('/logout')
    .then(res => {
        if(res.redirected)
            location.href = res.url;
    })
};

for(let e of loginForm.children){
    e.addEventListener('blur', () =>{
        setTimeout(() =>{
            if(document.activeElement.parentElement != loginForm){
                loginForm.style.display = 'none';
                let t = loginForm.querySelector('#error-text');
                if(t)
                    t.remove();
            }
        }, 10);
    });
}

function EditHandler(elem)
{
    document.forms[0].style.display = '';
    document.forms[0].dataset.id = elem.dataset.id;
    document.querySelector('#confirm-button').value = 'Применить';
    const elems = elem.parentElement.parentElement.querySelectorAll('td');
    document.querySelector('#student-fio').value = elems[1].innerHTML;
    document.querySelector('#student-course').value = elems[2].innerHTML;
    document.querySelector('#student-spec').value = elems[3].innerHTML;
    document.querySelector('#student-number').value = elems[4].innerHTML;
}